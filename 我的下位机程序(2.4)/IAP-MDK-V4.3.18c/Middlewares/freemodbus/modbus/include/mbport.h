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
 * File: $Id: mbport.h,v 1.19 2010/06/06 13:54:40 wolti Exp $
 */

#ifndef _MB_PORT_H
#define _MB_PORT_H

#ifdef __cplusplus
PR_BEGIN_EXTERN_C
#endif

#include "mb_type.h"

/* ----------------------- Supporting functions -----------------------------*/
uint8_t xMBPortEventInit( MB_VAR_TypeDef *pMbVarStruct );
uint8_t xMBPortEventPost( MB_VAR_TypeDef *pMbVarStruct, uint8_t ucQueue, eMBEventType eEvent );
uint8_t xMBPortEventGet(  MB_VAR_TypeDef *pMbVarStruct, eMBEventType * eEvent );

/* ----------------------- Serial port functions ----------------------------*/

uint8_t xMBPortSerialInit( uint8_t ucPort, uint32_t ulBaudRate, uint8_t ucDataBits, eMBParity eParity );
void    vMBPortClose( MB_VAR_TypeDef *pMbVarStruct );
void    xMBPortSerialClose( void );
void    vMBPortSerialEnable( UART_HandleTypeDef *huart, uint8_t xRxEnable, uint8_t xTxEnable );
uint8_t xMBPortSerialGetByte( UART_HandleTypeDef *huart, uint8_t *pucByte );
uint8_t xMBPortSerialPutByte( UART_HandleTypeDef *huart, uint8_t *pData, uint16_t size );

/* ----------------------- Timers functions ---------------------------------*/
uint8_t xMBPortTimersInit(TIM_HandleTypeDef *htim, uint16_t usTimeOut50us );
void    xMBPortTimersClose( void );
//void    vMBPortTimersEnable( TIM_HandleTypeDef *htim );
//void    vMBPortTimersDisable( TIM_HandleTypeDef *htim );
void    vMBPortTimersDelay( uint16_t usTimeOutMS );

void vMBPortTimersEnable( MB_VAR_TypeDef *pMbVarStruct );
void vMBPortTimersDisable( MB_VAR_TypeDef *pMbVarStruct );
void vMBPortTimersFrame( MB_PORT_TypeDef *pMbPortStruct );

/* ----------------------- Callback for the protocol stack ------------------*/

/*!
 * \brief Callback function for the porting layer when a new byte is
 *   available.
 *
 * Depending upon the mode this callback function is used by the RTU or
 * ASCII transmission layers. In any case a call to xMBPortSerialGetByte()
 * must immediately return a new character.
 *
 * \return <code>TRUE</code> if a event was posted to the queue because
 *   a new byte was received. The port implementation should wake up the
 *   tasks which are currently blocked on the eventqueue.
 */

/* ----------------------- TCP port functions -------------------------------*/
uint8_t xMBTCPPortInit( uint16_t usTCPPort );
void    vMBTCPPortClose( MB_VAR_TypeDef *pMbVarStruct );
void    vMBTCPPortDisable( void );
uint8_t xMBTCPPortGetRequest( MB_VAR_TypeDef *pMbVarStruct, uint8_t **ppucMBTCPFrame, uint16_t * usTCPLength );
uint8_t xMBTCPPortSendResponse( MB_VAR_TypeDef *pMbVarStruct, const uint8_t *pucMBTCPFrame, uint16_t usTCPLength );
uint8_t xMBTCPRecv( MB_VAR_TypeDef *pMbVarStruct );

#ifdef __cplusplus
PR_END_EXTERN_C
#endif
#endif
