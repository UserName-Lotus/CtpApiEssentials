﻿#ifndef TGATE_FTDCDATATYPE_H
#define TGATE_FTDCDATATYPE_H

/////////////////////////////////////////////////////////////////////////
///TFtdcBrokerIDType是一个经纪公司代码类型
/////////////////////////////////////////////////////////////////////////
typedef char TTGateFtdcBrokerIDType[11];

/////////////////////////////////////////////////////////////////////////
///TFtdcUserIDType是一个用户代码类型
/////////////////////////////////////////////////////////////////////////
typedef char TTGateFtdcUserIDType[16];

/////////////////////////////////////////////////////////////////////////
///TFtdcIpAddrType是一个服务地址IP类型
/////////////////////////////////////////////////////////////////////////
typedef char TTGateFtdcIpAddrType[129];

/////////////////////////////////////////////////////////////////////////
///TFtdcDRIdentityIDType是一个交易中心代码类型
/////////////////////////////////////////////////////////////////////////
typedef int TTGateFtdcDRIdentityIDType;

/////////////////////////////////////////////////////////////////////////
///TFtdcDRIdentityNameType是一个交易中心名称类型
/////////////////////////////////////////////////////////////////////////
typedef char TTGateFtdcDRIdentityNameType[65];

/////////////////////////////////////////////////////////////////////////
///TFtdcAddrSrvModeType是一个地址服务类型类型
/////////////////////////////////////////////////////////////////////////
///交易地址
#define TTGATE_FTDC_ASM_Trade '0'
///行情地址
#define TTGATE_FTDC_ASM_MarketData '1'
///其他
#define THOST_FTDC_ASM_Other '2'

typedef char TTGateFtdcAddrSrvModeType;

/////////////////////////////////////////////////////////////////////////
///TFtdcAddrVerType是一个地址版本类型
/////////////////////////////////////////////////////////////////////////
///IPV4
#define TTGATE_FTDC_ADV_V4 '0'
///IPV6
#define TTGATE_FTDC_ADV_V6 '1'

typedef char TTGateFtdcAddrVerType;

/////////////////////////////////////////////////////////////////////////
///TFtdcBoolType是一个布尔型类型
/////////////////////////////////////////////////////////////////////////
typedef int TTGateFtdcBoolType;

/////////////////////////////////////////////////////////////////////////
///TFtdcAddrRemarkType是一个地址备注类型
/////////////////////////////////////////////////////////////////////////
typedef char TTGateFtdcAddrRemarkType[161];

/////////////////////////////////////////////////////////////////////////
///TFtdcErrorIDType是一个错误代码类型
/////////////////////////////////////////////////////////////////////////
typedef int TTGateFtdcErrorIDType;

/////////////////////////////////////////////////////////////////////////
///TFtdcErrorMsgType是一个错误信息类型
/////////////////////////////////////////////////////////////////////////
typedef char TTGateFtdcErrorMsgType[81];

/////////////////////////////////////////////////////////////////////////
///TFtdcAddrNameType是一个地址名称类型
/////////////////////////////////////////////////////////////////////////
typedef char TTGateFtdcAddrNameType[65];

/////////////////////////////////////////////////////////////////////////
///TFtdcCommonIntType是一个通用int类型类型
/////////////////////////////////////////////////////////////////////////
typedef int TTGateFtdcCommonIntType;


#endif 