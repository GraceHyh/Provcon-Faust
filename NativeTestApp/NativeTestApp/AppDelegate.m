//
//  Author:
//      Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#import "AppDelegate.h"
#import <Security/Security.h>

@implementation AppDelegate

- (void)dealloc
{
    [super dealloc];
}

- (void) getPassword {
	const char *server = "192.168.16.101";
	const char *user = "mono";
	int pwLength = 0;
	void *password;
	SecKeychainItemRef item;
	OSStatus status = SecKeychainFindInternetPassword (
		NULL, strlen(server), server, 0, NULL, strlen (user), user, 0, NULL, 3128,
		kSecProtocolTypeAny, kSecAuthenticationTypeAny,
		&pwLength, &password, &item);
	printf("TEST: %x - %d - %p\n", status, pwLength, password);
	CFStringRef error = SecCopyErrorMessageString(status, NULL);
	printf("ERROR: %s\n", CFStringGetCStringPtr(error, kCFStringEncodingASCII));
	
}

- (NSData *)saveImage:(NSImage *)image location:(NSString *)location quality:(float)quality {
	NSBitmapImageRep *imageRep = [[image representations] objectAtIndex:0];
	NSDictionary *dict = [[NSDictionary alloc] initWithObjectsAndKeys:[NSNumber numberWithFloat:quality], NSImageCompressionFactor, nil];
	
	NSData *data = [imageRep representationUsingType:NSJPEGFileType properties:dict];
	printf("DATA: %ld\n", [data length]);
	return data;
}

- (void)dumpDictionary:(CFDictionaryRef)dictionary {
	int count = (int)CFDictionaryGetCount(dictionary);
	void **keys, **values;
	printf ("DUMPING DICTIONARY: %d - %p\n", count, dictionary);
	
	keys = alloca((count+1) * sizeof (void*));
	values = alloca((count+1) * sizeof (void*));
	CFDictionaryGetKeysAndValues(dictionary, keys, values);
	for (int i = 0; i < count; i++) {
		NSString* string = (NSString*)keys[i];
		const char *key = [string cString];
		printf("DICT: %p - %p - %s\n", keys[i], values[i], key);
		if (!strcmp(key, "HTTPUser")) {
			NSString *value = (NSString*)values[i];
			printf("VALUE: %s\n", [value cString]);
		}
	}
	printf("DONE!\n");
}

- (void)getProxy {
	CFDictionaryRef proxySettings = CFNetworkCopySystemProxySettings();
	[self dumpDictionary:proxySettings];
	
	CFStringRef urlString = CFStringCreateWithCString(NULL, "http://www.heise.de/", kCFStringEncodingUTF8);
	CFURLRef url = CFURLCreateWithString(NULL, urlString, NULL);
	
	CFArrayRef proxies = CFNetworkCopyProxiesForURL(url, proxySettings);
	printf("PROXIES: %p\n", proxies);
	int count = (int)CFArrayGetCount(proxies);
	
	for (int i = 0; i < count; i++) {
		CFDictionaryRef proxy = (CFDictionaryRef)CFArrayGetValueAtIndex(proxies, i);
		printf("PROXY: %p\n", proxy);
		[self dumpDictionary:proxy];
		void *user = CFDictionaryGetValue(proxy, kCFProxyUsernameKey);
		printf("PROXY USER: %p - %p\n", kCFProxyUsernameKey, user);
	}
	
	printf("ALL DONE!\n");
}

- (void)testNetwork {
	[self getPassword];
	[self getProxy];
	NSURL *url = [[NSURL alloc] initWithString:@"http://www.heise.de"];
	NSURLRequest *request = [[NSURLRequest alloc] initWithURL:url];
	NSURLResponse *response;
	NSError *error = NULL;

	[NSURLConnection sendSynchronousRequest:request returningResponse:&response error:&error];
	printf("DONE: %p - %p\n", response, error);
	if (error) {
		NSString *text = [error localizedDescription];
		NSString *text2 = [error localizedFailureReason];
		printf ("ERROR: %s - %s\n", [text cString], [text2 cString]);
	}
	printf("DONE!\n");
}

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification
{
	[self testNetwork];
}

@end
