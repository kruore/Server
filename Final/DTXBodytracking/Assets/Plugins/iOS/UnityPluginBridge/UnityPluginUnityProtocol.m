//
//  UnityPluginUnityProtocol.m
//  UnityPluginBridge
//
//  Created by KINLAB on 2022/08/05.
//

#import <Foundation/Foundation.h>
#import "UnityPluginUnityProtocol.h"

@implementation  UnityPluginUnityProtocol

- (void)Return_ProviderListWithLine:(NSString * _Nonnull)line {
    UnitySendMessage("iOSNative","__fromnative_Request_ProviderList",[line UTF8String]);
}

- (void)Return_selfNumberWithNumber:(NSString * _Nonnull)number {
    UnitySendMessage("iOSNative","__fromnative_selfNumber",[number UTF8String]);
}

- (void)Return_providerDisconnectWithNumber:(NSString * _Nonnull)number {
    UnitySendMessage("iOSNative","__fromnative_disconnectProvider",[number UTF8String]);
}

- (void)Return_bindNumberWithNumber:(NSString * _Nonnull)number {
    UnitySendMessage("iOSNative","__fromnative_bindNumber",[number UTF8String]);
}

- (void)Return_providerLabelTime {
    UnitySendMessage("iOSNative","__fromnative_Request_LabelSetting","");
}

- (void)Set_ProviderLabelTimeWithNumber:(NSString * _Nonnull)number {
    UnitySendMessage("iOSNative","__fromnative_LabelTimeSet",[number UTF8String]);
}

- (void)Set_ProviderLabelDataWithNumber:(NSString * _Nonnull)number {
    UnitySendMessage("iOSNative","__fromnative_LabelDataSend",[number UTF8String]);
}
@end
