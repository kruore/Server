//
//  UnityPluginWrapper.m
//  UnityPluginBridge
//
//  Created by KINLAB on 2022/08/05.
//

#import <Foundation/Foundation.h>
#import "AppleLogger/AppleLogger.h"
#import "UnityPluginWrapper.h"

#pragma mark - String Helpers
UnityPluginController* UPC;
static NSString * const NSStringFromCString(const char *string)
{
    if(string !=NULL)
    {
        return [NSString stringWithUTF8String:string];
    }
    else
    {
        return nil;
    }
}

static const char *const CStringFromNSString(NSString *string)
{
    if(string != NULL)
    {
        return[string cStringUsingEncoding:NSUTF8StringEncoding];
    }
    else
    {
        return nil;
    }
}
#pragma  mark - C interface
extern "C"
{
    void __iOS_Initialize()
    {
        printf("IOS_INITIALIZECALL");
        UnityPluginUnityProtocol* callback = [[UnityPluginUnityProtocol alloc]init];
        [[UnityPluginController GetInstance]InitializeWithPluginDelegate:(id<UnityPluginProtocol> _Nonnull)callback];
    }
    void __iOS_TCPStart(const char* ipnumber, int port)
    {
        [[UnityPluginController GetInstance]DataSendStartWithLine:NSStringFromCString(ipnumber) port:port];
    }
    void __iOS_TCPEnd()
    {
        [[UnityPluginController GetInstance]DataSendEnd];
    }

    void __iOS_RequestGetProvider()
    {
        [[UnityPluginController GetInstance]Request_GetProviderList];
    }

    void __iOS_RequestBindProvider(const char* num)
    {
        [[UnityPluginController GetInstance]RequestBind_ProviderWithLine:NSStringFromCString(num)];
    }

    void __iOS_Set_LabelTime()
    {
        [[UnityPluginController GetInstance]Request_GetProviderTime];
    }
    void __iOS_SettingComp_LabelTime(double time)
    {
        [[UnityPluginController GetInstance]SettingComp_LabelTimeWithLine:time];
    }

    void __iOS_SetLabelString(const char * num)
    {
        [[UnityPluginController GetInstance]Request_ProviderLabelSendWithLine:NSStringFromCString(num)];
    }
}
