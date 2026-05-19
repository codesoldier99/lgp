/* 
 * FreeModbus Libary: A portable Modbus implementation for Modbus ASCII/RTU.
 * Copyright (c) 2006 Christian Walter <wolti@sil.at>
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * File: $Id: mb.c,v 1.28 2010/06/06 13:54:40 wolti Exp $
 */

/* ----------------------- System includes ----------------------------------*/
#include "stdlib.h"
#include "string.h"


/* ----------------------- Modbus includes ----------------------------------*/
#include "mb.h"
#include "mbconfig.h"
#include "mbframe.h"
#include "mbproto.h"
#include "mbfunc.h"

#include "mbport.h"
#if MB_RTU_ENABLED == 1
	#include "mbrtu.h"
#endif
#if MB_ASCII_ENABLED == 1
	#include "mbascii.h"
#endif
#if MB_TCP_ENABLED == 1
	#include "mbtcp.h"
#endif

#ifndef MB_PORT_HAS_CLOSE
#define MB_PORT_HAS_CLOSE 0
#endif

/* ----------------------- Static variables ---------------------------------*/

static uint8_t    ucMBAddress;
static eMBMode  eMBCurrentMode;

static enum
{
    STATE_ENABLED,
    STATE_DISABLED,
    STATE_NOT_INITIALIZED
} eMBState = STATE_NOT_INITIALIZED;

/* Callback functions required by the porting layer. They are called when
 * an external event has happend which includes a timeout or the reception
 * or transmission of a character.
 */


uint8_t( *pxMBFrameCBReceiveFSMCur ) ( void );
uint8_t( *pxMBFrameCBTransmitFSMCur ) ( void );

/* An array of Modbus functions handlers which associates Modbus function
 * codes with implementing functions.
 */
static xMBFunctionHandler xFuncHandlers[MB_FUNC_HANDLERS_MAX] = {
#if MB_FUNC_OTHER_REP_SLAVEID_ENABLED > 0
    {MB_FUNC_OTHER_REPORT_SLAVEID, eMBFuncReportSlaveID},
#endif
#if MB_FUNC_READ_INPUT_ENABLED > 0
    {MB_FUNC_READ_INPUT_REGISTER, eMBFuncReadInputRegister},
#endif
#if MB_FUNC_READ_HOLDING_ENABLED > 0
    {MB_FUNC_READ_HOLDING_REGISTER, eMBFuncReadHoldingRegister},
#endif
#if MB_FUNC_WRITE_MULTIPLE_HOLDING_ENABLED > 0
    {MB_FUNC_WRITE_MULTIPLE_REGISTERS, eMBFuncWriteMultipleHoldingRegister},
#endif
#if MB_FUNC_WRITE_HOLDING_ENABLED > 0
    {MB_FUNC_WRITE_REGISTER, eMBFuncWriteHoldingRegister},
#endif
#if MB_FUNC_READWRITE_HOLDING_ENABLED > 0
    {MB_FUNC_READWRITE_MULTIPLE_REGISTERS, eMBFuncReadWriteMultipleHoldingRegister},
#endif
#if MB_FUNC_READ_COILS_ENABLED > 0
    {MB_FUNC_READ_COILS, eMBFuncReadCoils},
#endif
#if MB_FUNC_WRITE_COIL_ENABLED > 0
    {MB_FUNC_WRITE_SINGLE_COIL, eMBFuncWriteCoil},
#endif
#if MB_FUNC_WRITE_MULTIPLE_COILS_ENABLED > 0
    {MB_FUNC_WRITE_MULTIPLE_COILS, eMBFuncWriteMultipleCoils},
#endif
#if MB_FUNC_READ_DISCRETE_INPUTS_ENABLED > 0
    {MB_FUNC_READ_DISCRETE_INPUTS, eMBFuncReadDiscreteInputs},
#endif
};

/* ----------------------- Start implementation -----------------------------*/
eMBErrorCode eMBInit( MB_PORT_TypeDef *pPortStruct,\
		eMBMode eMode, uint8_t ucSlaveAddress, uint8_t ucPort, uint32_t ulBaudRate, eMBParity eParity )
{
    
	MB_FUNC_TypeDef *p_func_struct = &pPortStruct->func_struct;
	MB_VAR_TypeDef  *p_var_struct  = &pPortStruct->var_struct;
	eMBErrorCode    eStatus = MB_ENOERR;
		
	
    /* check preconditions */
    if( ( ucSlaveAddress == MB_ADDRESS_BROADCAST ) ||
        ( ucSlaveAddress < MB_ADDRESS_MIN ) || ( ucSlaveAddress > MB_ADDRESS_MAX ) )
    {
        eStatus = MB_EINVAL;
    }
    else
    {
        ucMBAddress = ucSlaveAddress;

        switch ( eMode )
        {
#if MB_RTU_ENABLED > 0
        case MB_RTU:
            p_func_struct->pvMBFrameStart   = eMBRTUStart;
            p_func_struct->pvMBFrameStop    = eMBRTUStop;
            p_func_struct->peMBFrameSend    = eMBRTUSend;
            p_func_struct->peMBFrameReceive = eMBRTUReceive;	//完整帧内容分段处理函数地址赋值（从机地址 + 帧内容 + 帧长度）
            p_func_struct->pvMBFrameClose   = MB_PORT_HAS_CLOSE ? vMBPortClose : NULL;
            
						p_func_struct->pxMBFrameCBByteReceived     = xMBRTUReceiveFSM;			//接收函数地址赋值
						p_func_struct->pxMBFrameCBTransmitterEmpty = xMBRTUTransmitFSM;			//发送函数地址赋值
						p_func_struct->pxMBPortCBTimerExpired      = xMBRTUTimerT35Expired;	//T35计时函数地址赋值

            eStatus = eMBRTUInit(p_var_struct, ucMBAddress, ucPort, ulBaudRate, eParity);
            break;
#endif
#if MB_ASCII_ENABLED > 0
        case MB_ASCII:
            p_func_struct->pvMBFrameStart   = eMBASCIIStart;
            p_func_struct->pvMBFrameStop    = eMBASCIIStop;
            p_func_struct->peMBFrameSend    = eMBASCIISend;
            p_func_struct->peMBFrameReceive = eMBASCIIReceive;
						p_func_struct->pvMBFrameClose   = MB_PORT_HAS_CLOSE ? vMBPortClose : NULL;
            
						p_func_struct->pxMBFrameCBByteReceived     = xMBASCIIReceiveFSM;
            p_func_struct->pxMBFrameCBTransmitterEmpty = xMBASCIITransmitFSM;
            p_func_struct->pxMBPortCBTimerExpired      = xMBASCIITimerT1SExpired;

            eStatus = eMBASCIIInit(p_var_struct, ucMBAddress, ucPort, ulBaudRate, eParity );
            break;
#endif
        default:
            eStatus = MB_EINVAL;
        }

        if( eStatus == MB_ENOERR )
        {
            if( !xMBPortEventInit( p_var_struct ) )
            {
                /* port dependent event module initalization failed. */
                eStatus = MB_EPORTERR;
            }
            else
            {
                eMBCurrentMode = eMode;
                eMBState = STATE_DISABLED;
            }
        }
    }
    return eStatus;
}

#if MB_TCP_ENABLED > 0
eMBErrorCode eMBTCPInit( MB_PORT_TypeDef *pPortStruct, uint16_t ucTCPPort )
{
	MB_FUNC_TypeDef *p_func_struct = &pPortStruct->func_struct;
	MB_VAR_TypeDef  *p_var_struct  = &pPortStruct->var_struct;
	
	eMBErrorCode    eStatus = MB_ENOERR;

	if( ( eStatus = eMBTCPDoInit( p_var_struct, ucTCPPort ) ) != MB_ENOERR )
	{
			eMBState = STATE_DISABLED;
	}
	else if( !xMBPortEventInit( p_var_struct ) )
	{
			/* Port dependent event module initalization failed. */
			eStatus = MB_EPORTERR;
	}
	else
	{
			p_func_struct->pvMBFrameStart   = eMBTCPStart;
			p_func_struct->pvMBFrameStop    = eMBTCPStop;
			p_func_struct->peMBFrameReceive = eMBTCPReceive;
			p_func_struct->peMBFrameSend    = eMBTCPSend;
			p_func_struct->pvMBFrameClose   = MB_PORT_HAS_CLOSE ? vMBTCPPortClose : NULL;
			
			ucMBAddress = MB_TCP_PSEUDO_ADDRESS;
			eMBCurrentMode = MB_TCP;
			eMBState = STATE_DISABLED;
	}
	return eStatus;
}
#endif

eMBErrorCode eMBRegisterCB( uint8_t ucFunctionCode, pxMBFunctionHandler pxHandler )
{
    int             i;
    eMBErrorCode    eStatus;

    if( ( 0 < ucFunctionCode ) && ( ucFunctionCode <= 127 ) )
    {
        if( pxHandler != NULL )
        {
            for( i = 0; i < MB_FUNC_HANDLERS_MAX; i++ )
            {
                if( ( xFuncHandlers[i].pxHandler == NULL ) || ( xFuncHandlers[i].pxHandler == pxHandler ) )
                {
                    xFuncHandlers[i].ucFunctionCode = ucFunctionCode;
                    xFuncHandlers[i].pxHandler = pxHandler;
                    break;
                }
            }
            eStatus = ( i != MB_FUNC_HANDLERS_MAX ) ? MB_ENOERR : MB_ENORES;
        }
        else
        {
            for( i = 0; i < MB_FUNC_HANDLERS_MAX; i++ )
            {
                if( xFuncHandlers[i].ucFunctionCode == ucFunctionCode )
                {
                    xFuncHandlers[i].ucFunctionCode = 0;
                    xFuncHandlers[i].pxHandler = NULL;
                    break;
                }
            }
            /* Remove can't fail. */
            eStatus = MB_ENOERR;
        }
    }
    else
    {
        eStatus = MB_EINVAL;
    }
    return eStatus;
}


eMBErrorCode eMBClose( MB_PORT_TypeDef *pMbPortStruct )
{
  MB_FUNC_TypeDef *p_func_struct = &pMbPortStruct->func_struct;  
	eMBErrorCode    eStatus = MB_ENOERR;
		
	if( eMBState == STATE_DISABLED ){
		if( p_func_struct->pvMBFrameClose != NULL ){
				p_func_struct->pvMBFrameClose( &pMbPortStruct->var_struct );
		}
	}else{
		eStatus = MB_EILLSTATE;
	}
	return eStatus;
}

eMBErrorCode eMBEnable( MB_PORT_TypeDef *pMbPortStruct )
{
  MB_FUNC_TypeDef *p_func_struct = &pMbPortStruct->func_struct;    
	eMBErrorCode    eStatus = MB_ENOERR;

	if( eMBState == STATE_DISABLED ){
			p_func_struct->pvMBFrameStart( &pMbPortStruct->var_struct );	/* Activate the protocol stack. */
			eMBState = STATE_ENABLED;
	}else{
			eStatus = MB_EILLSTATE;
	}
	return eStatus;
}

eMBErrorCode eMBDisable( MB_PORT_TypeDef *pMbPortStruct )
{
  MB_FUNC_TypeDef *p_func_struct = &pMbPortStruct->func_struct;   
	eMBErrorCode    eStatus;

	if( eMBState == STATE_ENABLED ){
		p_func_struct->pvMBFrameStop( &pMbPortStruct->var_struct );
		eMBState = STATE_DISABLED;
		eStatus = MB_ENOERR;
	}else if( eMBState == STATE_DISABLED ){
		eStatus = MB_ENOERR;
	}else{
		eStatus = MB_EILLSTATE;
	}
	return eStatus;
}

/*
* main函数 while(1) 循环扫描
* 完整帧内容解析
*/
eMBErrorCode eMBPoll( MB_PORT_TypeDef *pMbPortStruct )
{
    static uint8_t   *ucMBFrame;
    static uint8_t    ucRcvAddress;
    static uint8_t    ucFunctionCode;
    static uint16_t   usLength;
    static eMBException eException;
    int             i;
    
		MB_FUNC_TypeDef *p_func_struct = &pMbPortStruct->func_struct;
		eMBErrorCode    eStatus = MB_ENOERR;
    eMBEventType    eEvent;

    /* Check if the protocol stack is ready. */
    if( eMBState != STATE_ENABLED )
    {
        return MB_EILLSTATE;
    }

		/* 判断是否含有完整帧 */
		vMBPortTimersFrame(pMbPortStruct);
		
    /* Check if there is a event available. If not return control to caller.
     * Otherwise we will handle the event. */
    if( xMBPortEventGet( &pMbPortStruct->var_struct, &eEvent ) == TRUE )
    {
        switch ( eEvent )
        {
					case EV_READY:
					
					break;

					case EV_FRAME_RECEIVED:	//完整帧分段，同时校验从机地址
						eStatus = p_func_struct->peMBFrameReceive( &pMbPortStruct->var_struct, &ucRcvAddress, &ucMBFrame, &usLength );	//完整帧分段
						if( eStatus == MB_ENOERR ){	//错误: 长度超过配置 或 校验错误
							/* Check if the frame is for us. If not ignore the frame. */
							if( ( ucRcvAddress == ucMBAddress ) || ( ucRcvAddress == MB_ADDRESS_BROADCAST ) ){
								( void )xMBPortEventPost( &pMbPortStruct->var_struct, TRUE, EV_EXECUTE );
								break;
							}
						}
						( void )xMBPortEventPost( &pMbPortStruct->var_struct, FALSE, EV_FRAME_ERROR );
						
					break;

					case EV_EXECUTE:
						ucFunctionCode = ucMBFrame[MB_PDU_FUNC_OFF];	//功能码
						eException = MB_EX_ILLEGAL_FUNCTION;
						for( i = 0; i < MB_FUNC_HANDLERS_MAX; i++ )		//功能码对应执行函数查找
						{
								/* No more function handlers registered. Abort. */
								if( xFuncHandlers[i].ucFunctionCode == 0 ){
										break;
								}else if( xFuncHandlers[i].ucFunctionCode == ucFunctionCode ){
										eException = xFuncHandlers[i].pxHandler( ucMBFrame, &usLength, &pMbPortStruct->var_struct);
										break;
								}
						}

						/* If the request was not sent to the broadcast address we return a reply. */
						if( ucRcvAddress != MB_ADDRESS_BROADCAST ){
								if( eException != MB_EX_NONE ){
									/* An exception occured. Build an error frame. */
									usLength = 0;
									ucMBFrame[usLength++] = ( uint8_t )( ucFunctionCode | MB_FUNC_ERROR );
									ucMBFrame[usLength++] = eException;
								}
								
								if( ( eMBCurrentMode == MB_ASCII ) && MB_ASCII_TIMEOUT_WAIT_BEFORE_SEND_MS ){
									vMBPortTimersDelay( MB_ASCII_TIMEOUT_WAIT_BEFORE_SEND_MS );
								}                
								eStatus = p_func_struct->peMBFrameSend( &pMbPortStruct->var_struct, ucMBAddress, ucMBFrame, usLength );
						}
					break;

					case EV_FRAME_SENT:
						p_func_struct->pxMBFrameCBTransmitterEmpty(  &pMbPortStruct->var_struct );
					break;
        }
    }
    return MB_ENOERR;
}
