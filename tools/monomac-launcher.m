// gcc -Wall -Werror -mmacosx-version-min=10.6 -m32 -o monomac-launcher monomac-launcher.m -framework AppKit

//
// This is based heavily on monodevelop/main/build/MacOSX/monostub.m
//

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <sys/resource.h>
#include <unistd.h>
#include <dlfcn.h>
#include <errno.h>
#include <ctype.h>

#import <Cocoa/Cocoa.h>

// command-line arguments
static const char *appPathArg;
static const char *monoPathArg;
static const char *frameworkArg;
static const char *exeArg;
// computed
static const char *basename;
static const char *appName;
static const char *appDir;
static const char *monoLibPath;
static const char *exePath;

typedef int (* mono_main) (int argc, char **argv);
typedef void (* mono_free) (void *ptr);
typedef char * (* mono_get_runtime_build_info) (void);

static void
exit_with_message (char *reason, char *argv0)
{
	NSString *appName = nil;
	NSDictionary *plist = [[NSBundle mainBundle] infoDictionary];
	if (plist) {
		appName = (NSString *) [plist objectForKey:(NSString *)kCFBundleNameKey];
	}
	if (!appName) {
		appName = [[NSString stringWithUTF8String: argv0] lastPathComponent];
	}

	NSAlert *alert = [[NSAlert alloc] init];
	[alert setMessageText:[NSString stringWithFormat:@"Could not launch %@", appName]];
	NSString *fmt = @"%s\n\nPlease download and install the latest version of Mono.";
	NSString *msg = [NSString stringWithFormat:fmt, reason]; 
	[alert setInformativeText:msg];
	[alert addButtonWithTitle:@"Download Mono Framework"];
	[alert addButtonWithTitle:@"Cancel"];
	NSInteger answer = [alert runModal];
	[alert release];
	
	if (answer == NSAlertFirstButtonReturn) {
		NSString *mono_download_url = @"http://www.go-mono.com/mono-downloads/download.html";
		CFURLRef url = CFURLCreateWithString (NULL, (CFStringRef) mono_download_url, NULL);
		LSOpenCFURLRef (url, NULL);
		CFRelease (url);
	}
	exit (1);
}

static void
fatal_error (char *reason)
{
	fprintf (stderr, "Fatal Error: %s\n", reason);
	exit (1);
}

static int
check_mono_version (const char *version, const char *req_version)
{
	char *req_end, *end;
	long req_val, val;
	
	while (*req_version) {
		req_val = strtol (req_version, &req_end, 10);
		if (req_version == req_end || (*req_end && *req_end != '.')) {
			fprintf (stderr, "Bad version requirement string '%s'\n", req_end);
			return FALSE;
		}
		
		req_version = req_end;
		if (*req_version)
			req_version++;
		
		val = strtol (version, &end, 10);
		if (version == end || val < req_val)
			return FALSE;
		
		if (val > req_val)
			return TRUE;
		
		if (*req_version == '.' && *end != '.')
			return FALSE;
		
		version = end + 1;
	}
	
	return TRUE;
}

typedef struct _ListNode {
	struct _ListNode *next;
	char *value;
} ListNode;

static char *
decode_qstring (unsigned char **in, unsigned char qchar)
{
	unsigned char *inptr = *in;
	unsigned char *start = *in;
	char *value, *v;
	size_t len = 0;
	
	while (*inptr) {
		if (*inptr == qchar)
			break;
		
		if (*inptr == '\\') {
			if (inptr[1] == '\0')
				break;
			
			inptr++;
		}
		
		inptr++;
		len++;
	}
	
	v = value = (char *) malloc (len + 1);
	while (start < inptr) {
		if (*start == '\\')
			start++;
		
		*v++ = (char) *start++;
	}
	
	*v = '\0';
	
	if (*inptr)
		inptr++;
	
	*in = inptr;
	
	return value;
}

static char **
get_mono_env_options (int *count)
{
	const char *env = getenv ("MONO_ENV_OPTIONS");
	ListNode *list = NULL, *node, *tail = NULL;
	unsigned char *start, *inptr;
	char *value, **argv;
	int i, n = 0;
	size_t size;
	
	if (env == NULL) {
		*count = 0;
		return NULL;
	}
	
	inptr = (unsigned char *) env;
	
	while (*inptr) {
		while (isblank ((int) *inptr))
			inptr++;
		
		if (*inptr == '\0')
			break;
		
		start = inptr++;
		switch (*start) {
		case '\'':
		case '"':
			value = decode_qstring (&inptr, *start);
			break;
		default:
			while (*inptr && !isblank ((int) *inptr))
				inptr++;
			
			// Note: Mac OS X <= 10.6.8 do not have strndup()
			//value = strndup ((char *) start, (size_t) (inptr - start));
			size = (size_t) (inptr - start);
			value = (char *) malloc (size + 1);
			memcpy (value, start, size);
			value[size] = '\0';
			break;
		}
		
		node = (ListNode *) malloc (sizeof (ListNode));
		node->value = value;
		node->next = NULL;
		n++;
		
		if (tail != NULL)
			tail->next = node;
		else
			list = node;
		
		tail = node;
	}
	
	*count = n;
	
	if (n == 0)
		return NULL;
	
	argv = (char **) malloc (sizeof (char *) * (n + 1));
	i = 0;
	
	while (list != NULL) {
		node = list->next;
		argv[i++] = list->value;
		free (list);
		list = node;
	}
	
	argv[i] = NULL;
	
	return argv;
}

static int
push_env (const char *variable, const char *value)
{
	size_t len = strlen (value);
	const char *current;
	int rv;
	
	if ((current = getenv (variable)) && *current) {
		char *buf = malloc (len + strlen (current) + 2);
		memcpy (buf, value, len);
		buf[len] = ':';
		strcpy (buf + len + 1, current);
		rv = setenv (variable, buf, 1);
		free (buf);
	} else {
		rv = setenv (variable, value, 1);
	}
	
	return rv;
}

static char *
str_append (const char *base, const char *append)
{
	size_t baselen = strlen (base);
	size_t len = strlen (append);
	char *buf;
	
	if (!(buf = malloc (baselen + len + 1)))
		return NULL;
	
	memcpy (buf, base, baselen);
	strcpy (buf + baselen, append);
	
	return buf;
}

static char *
launcher_variable (const char *app_name)
{
	char *variable = malloc (strlen (app_name) + 10);
	const char *s = app_name;
	char *d = variable;
	
	while (*s != '\0') {
		*d++ = (*s >= 'a' && *s <= 'z') ? *s - 0x20 : *s;
		s++;
	}
	
	strcpy (d, "_LAUNCHER");
	
	return variable;
}

static const char *
copy_to_utf8_str (NSString *str)
{
	int len = [str length];
	char *retval = (char*)malloc (len + 1);
	strncpy (retval, [str UTF8String], len);
	retval [len] = 0;
	return retval;
}

static void
update_environment (void)
{
	char *value, *v1, *v2;
	char *variable;
	char buf[32];

	const char *libraryPath;
	const char *gacPrefix;
	if (frameworkArg) {
		libraryPath = [[NSString stringWithFormat:@"/Library/Frameworks/Mono.Framework/Versions/%s/lib:/lib:/usr/lib", frameworkArg] UTF8String];
		gacPrefix = [[NSString stringWithFormat:@"/Library/Frameworks/Mono.Framework/Versions/%s", frameworkArg] UTF8String];
	} else {
		libraryPath = [[NSString stringWithFormat:@"%s/lib:/Library/Frameworks/Mono.Framework/Versions/Current/lib:/lib:/usr/lib", monoPathArg] UTF8String];
		gacPrefix = [[NSString stringWithFormat:@"%s:/Library/Frameworks/Mono.Framework/Versions/Current", monoPathArg] UTF8String];
	}
	
	push_env ("DYLD_FALLBACK_LIBRARY_PATH", libraryPath);
	
	/* Enable the use of stuff bundled into the app bundle */
	if ((v2 = str_append (appDir, "/share/pkgconfig"))) {
		if ((v1 = str_append (appDir, "/lib/pkgconfig:"))) {
			if ((value = str_append (v1, v2))) {
				push_env ("PKG_CONFIG_PATH", value);
				free (value);
			}
			
			free (v1);
		}
		
		free (v2);
	}
	
	push_env ("MONO_GAC_PREFIX", gacPrefix);
	
	/* Mono "External" directory */
	push_env ("PKG_CONFIG_PATH", "/Library/Frameworks/Mono.framework/External/pkgconfig");
	
	/* Set our launcher pid so we don't recurse */
	sprintf (buf, "%ld", (long) getpid ());
	variable = launcher_variable (basename);
	setenv (variable, buf, 1);
	free (variable);
}

static int
is_launcher (const char *app)
{
	char *variable = launcher_variable (app);
	const char *launcher;
	char buf[32];
	
	launcher = getenv (variable);
	free (variable);
	
	if (!(launcher && *launcher))
		return 1;
	
	sprintf (buf, "%ld", (long) getppid ());
	
	return !strcmp (launcher, buf);
}

static void
parse_args (int argc, char **argv, int *pos)
{
	while (*pos < argc) {
		if (!strncmp (argv [*pos], "--app=", 6)) {
			if (appPathArg)
				fatal_error ("Duplicate --app argument");
			appPathArg = argv [(*pos)++]+6;
		} else if (!strncmp (argv [*pos], "--mono=", 7)) {
			if (frameworkArg)
				fatal_error ("Cannot use both --mono and --framework");
			if (monoPathArg)
				fatal_error ("Duplicate --mono argument");
			monoPathArg = argv [(*pos)++]+7;
		} else if (!strncmp (argv [*pos], "--framework=", 12)) {
			if (monoPathArg)
				fatal_error ("Cannot use both --mono and --framework");
			if (frameworkArg)
				fatal_error ("Duplicate --framework argument");
			frameworkArg = argv [(*pos)++]+12;
		} else if (!strncmp (argv [*pos], "--exe=", 6)) {
			if (exeArg)
				fatal_error ("Duplicate --exe argument");
			exeArg = argv [(*pos)++]+6;
		} else {
			break;
		}
	}
	
	if (appPathArg) {
		const char *realAppPath = realpath (appPathArg, NULL);
		if (!realAppPath)
			fatal_error ("Invalid --app argument");
		basename = strrchr (realAppPath, '/');
		if (!basename)
			fatal_error ("Invalid --app argument");
		basename++;
		int len = strlen (basename);
		if ((len < 5) || strcmp (basename + len - 4, ".app"))
			fatal_error ("Invalid --app argument");
			
		appName = (char*)malloc (len - 3);
		strncpy ((char*)appName, basename, len - 4);
		*((char*)appName + len - 4) = 0;
		
		appDir = realAppPath;
	} else {
		const char *realAppPath = realpath (argv [0], NULL);
		basename = strrchr (realAppPath, '/');
		if (!basename)
			fatal_error ("Cannot get base name");
		basename++;
		appName = basename;
		
		appDir = copy_to_utf8_str ([[NSBundle mainBundle] bundlePath]);
	}
	
	NSString *path;
	if (monoPathArg)
		path = [NSString stringWithUTF8String: monoPathArg];
	else {
		if (!frameworkArg)
			frameworkArg = "Current";
		NSString *basedir = [NSString stringWithUTF8String: "/Library/Frameworks/Mono.Framework/Versions"];
		path = [basedir stringByAppendingPathComponent: [NSString stringWithUTF8String: frameworkArg]];
	}
	
	NSString *libPath = [path stringByAppendingPathComponent:@"lib"];
	monoLibPath = copy_to_utf8_str (libPath);
	
	if (!exeArg) {
		NSString *contents = [[NSString stringWithUTF8String: appDir] stringByAppendingPathComponent: @"Contents"];
		NSString *binDir = [contents stringByAppendingPathComponent: @"MonoBundle"];
		NSString *exeName = [NSString stringWithFormat:@"%s.exe", appName];
		exePath = copy_to_utf8_str ([binDir stringByAppendingPathComponent: exeName]);
	} else {
		exePath = exeArg;
	}
}

int main (int argc, char **argv)
{
	NSAutoreleasePool *pool = [[NSAutoreleasePool alloc] init];
	const char *req_mono_version = "2.10.9";
	struct rlimit limit;
	char **extra_argv;
	int first_argc = 1;
	int extra_argc;
	int i;
	
	parse_args (argc, argv, &first_argc);
	
	if (is_launcher (basename)) {
		update_environment ();
		
		[pool drain];
		return execv (argv[0], argv);
	}
	
	if (getrlimit (RLIMIT_NOFILE, &limit) == 0 && limit.rlim_cur < 1024) {
		limit.rlim_cur = MIN (limit.rlim_max, 1024);
		setrlimit (RLIMIT_NOFILE, &limit);
	}

	bool sgen = getenv ("MONODEVELOP_USE_SGEN") != NULL;
	NSString *libmonoName = [NSString stringWithUTF8String: sgen ? "libmonosgen-2.0.dylib" : "libmono-2.0.dylib"];
	const char *libmonoPath = [[[NSString stringWithUTF8String: monoLibPath] stringByAppendingPathComponent:libmonoName] UTF8String];
	void *libmono = dlopen (libmonoPath, RTLD_LAZY);
	
	if (libmono == NULL) {
		fprintf (stderr, "Failed to load libmono%s-2.0.dylib: %s\n", sgen ? "sgen" : "", dlerror ());
		exit_with_message ("This application requires the Mono framework.", argv[0]);
	}
	
	mono_main _mono_main = (mono_main) dlsym (libmono, "mono_main");
	if (!_mono_main) {
		fprintf (stderr, "Could not load mono_main(): %s\n", dlerror ());
		exit_with_message ("Failed to load the Mono framework.", argv[0]);
	}
	
	mono_free _mono_free = (mono_free) dlsym (libmono, "mono_free");
	if (!_mono_free) {
		fprintf (stderr, "Could not load mono_free(): %s\n", dlerror ());
		exit_with_message ("Failed to load the Mono framework.", argv[0]);
	}
	
	mono_get_runtime_build_info _mono_get_runtime_build_info = (mono_get_runtime_build_info) dlsym (libmono, "mono_get_runtime_build_info");
	if (!_mono_get_runtime_build_info) {
		fprintf (stderr, "Could not load mono_get_runtime_build_info(): %s\n", dlerror ());
		exit_with_message ("Failed to load the Mono framework.", argv[0]);
	}
	
	char *mono_version = _mono_get_runtime_build_info ();
	if (!check_mono_version (mono_version, req_mono_version))
		exit_with_message ("This application requires a newer version of the Mono framework.", argv[0]);
	
	extra_argv = get_mono_env_options (&extra_argc);
	
	const int injected = 2; /* --debug and exe path */
	int new_argc = (argc - first_argc) + extra_argc + injected + 1;
	char **new_argv = (char **) malloc (sizeof (char *) * (new_argc + 1));
	int n = 0;
	
	new_argv[n++] = (char*)appPathArg;
	for (i = 0; i < extra_argc; i++)
		new_argv[n++] = extra_argv[i];
	
	// enable --debug so that we can get useful stack traces
	new_argv[n++] = "--debug";
	
	new_argv[n++] = strdup (exePath);
	
	for (i = first_argc; i < argc; i++)
		new_argv[n++] = argv[i];
	new_argv[n] = NULL;
	
	free (extra_argv);
	[pool drain];
	
	return _mono_main (new_argc, new_argv);
}
