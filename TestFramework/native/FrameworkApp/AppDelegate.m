//
//  AppDelegate.m
//  FrameworkApp
//
//  Created by Martin Baulig on 10/12/12.
//  Copyright (c) 2012 Martin Baulig. All rights reserved.
//

#import "AppDelegate.h"
#include "NativeTest.h"
#include <dlfcn.h>

@implementation AppDelegate

- (void)dealloc
{
    [super dealloc];
}

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification
{
	void* framework = dlopen ("@executable_path/../Frameworks/TestFramework.framework/TestFramework", RTLD_NOW);
	printf ("FRAMEWORK: %p\n", framework);
	native_test_hello();
}

@end
