//
//  ProxySettings.m
//  NativeProxyTest
//
//  Created by Martin Baulig on 12/3/12.
//  Copyright (c) 2012 Xamarin. All rights reserved.
//

#import "ProxySettings.h"
#include <CFNetwork/CFNetwork.h>
#import <Security/Security.h>

@implementation ProxySettings

- (void)getPassword {
	NSMutableDictionary *dict = [[NSMutableDictionary alloc] init];
	[dict setValue:kSecClassInternetPassword forKey:kSecClass];
	//[dict setValue:@"mono" forKey:kSecAttrAccount];
	//[dict setValue:@"192.168.16.101" forKey:kSecAttrServer];
	[dict setValue:kSecMatchLimitAll forKey:kSecMatchLimit];
	[dict setValue:kCFBooleanTrue forKey:kSecReturnRef];
	
	CFTypeRef result = NULL;
	OSStatus status = SecItemCopyMatching (dict, &result);
	printf("DONE: %lx - %p\n", status, result);
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
		if (!strcmp(key, "HTTPProxyUsername")) {
			NSString *value = (NSString*)values[i];
			printf("PROXY USERNAME: %s\n", [value cString]);
		}
	}
	printf("DONE!\n");
}

- (void)getProxy {
	[self getPassword];
	return;
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

- (void)getSettings {
	
}

@end
