//
//  AppDelegate.h
//  NativeDelegateTest
//
//  Created by Martin Baulig on 10/29/12.
//  Copyright (c) 2012 Martin Baulig. All rights reserved.
//

#import <Cocoa/Cocoa.h>

@interface AppDelegate : NSObject <NSApplicationDelegate>

@property (assign) IBOutlet NSWindow *Window;
@property (assign) IBOutlet NSTextView *TextView;

@end
