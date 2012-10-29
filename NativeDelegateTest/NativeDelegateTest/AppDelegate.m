//
//  AppDelegate.m
//  NativeDelegateTest
//
//  Created by Martin Baulig on 10/29/12.
//  Copyright (c) 2012 Martin Baulig. All rights reserved.
//

#import "AppDelegate.h"
#import "MyDelegate.h"

@implementation AppDelegate

- (void)dealloc
{
    [super dealloc];
}

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification
{
	MyDelegate* dlg = [[MyDelegate alloc] init];
	[[self TextView] setDelegate:dlg];
}

@end
