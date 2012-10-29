//
//  MyDelegate.m
//  NativeDelegateTest
//
//  Created by Martin Baulig on 10/29/12.
//  Copyright (c) 2012 Martin Baulig. All rights reserved.
//

#import "MyDelegate.h"

@implementation MyDelegate

- (void)textViewDidChangeSelection:(NSNotification *)aNotification {
	printf("TEXT VIEW DID CHANGE SELECTION!\n");
}

- (void)textDidChange:(NSNotification *)aNotification {
	printf ("TEXT DID CHANGE!\n");
}

@end
