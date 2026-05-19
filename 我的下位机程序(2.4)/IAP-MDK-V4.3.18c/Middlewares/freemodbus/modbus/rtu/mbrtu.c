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
 * File: $Id: mbrtu.c,v 1.18 2007/09/12 10:15:56 wolti Exp $
 */

/* ----------------------- System includes ----------------------------------*/
#include "stdlib.h"
#include "string.h"

/* ----------------------- Modbus includes ----------------------------------*/
#include "mb.h"
#include "mbrtu.h"
#include "mbframe.h"

#include "mbcrc.h"
#include "mbport.h"

/* ----------------------- Defines ------------------------------------------*/
#define MB_SER_PDU_SIZE_MIN     4       /*!< Minimum size of a Modbus RTU frame. */
#define MB_SER_PDU_SIZE_MAX     64     /*!< Maximum size of a Modbus RTU frame. */
#define MB_SER_PDU_SIZE_CRC     2       /*!< Size of CRC field in PDU. */
#define MB_SER_PDU_ADDR_OFF     0       /*!< Offset of slave address in Ser-PDU. */
#define MB_SER_PDU_PDU_OFF      1       /*!< Offset of Modbus-PDU in Ser-PDU. */

/* ----------------------- Start implementation -----------------------------*/
eMBErrorCode eMBRTUInit(MB_VAR_TypeDef *pMbVarStruct, uint8_t ucSlaveAddress, uint8_t ucPort, uint32_t ulBaudRate, eMBParity eParity)
{
    eMBErrorCode    eStatus = MB_ENOERR;
  
    ( void )ucSlaveAddress;

    /* Modbus RTU uses 8 Databits. */
    if( xMBPortSerialInit( ucPort, ulBaudRate, 8, eParity ) != TRUE ){
        eStatus = MB_EPORTERR;
    }else{
			/* If baudrate > 19200（1800us） then we should use the fixed timer values
			 * t35 = 1750us. Otherwise t35 must be 3.5 times the character time.
       */
			if( ulBaudRate > 19200 ){
					pMbVarStruct->timer_value = 35*1000/ulBaudRate + 1;
				}else{
					pMbVarStruct->timer_value = 2;
				}
    }

    return eStatus;
}

void eMBRTUStart( MB_VAR_TypeDef *pMbVarStruct )
{
    /* Initially the receiver is in the state STATE_RX_INIT. we start
     * the timer and if no character is received within t3.5 we change
     * to STATE_RX_IDLE. This makes sure that we delay startup of the
     * modbus protocol stack until the bus is free.
     */
	pMbVarStruct->eRcvState = STATE_RX_INIT;
	vMBPortSerialEnable( pMbVarStruct->huart, TRUE, FALSE );
	vMBPortTimersEnable(pMbVarStruct);
}

void eMBRTUStop( MB_VAR_TypeDef *pMbVarStruct )
{
  vMBPortSerialEnable( pMbVarStruct->huart, FALSE, FALSE );
	vMBPortTimersDisable( pMbVarStruct );
}

/*
* 完整帧，内容分段（从机地址 + 帧内容 + 帧长度），接收数据缓存，同时校验位核定
*/
eMBErrorCode eMBRTUReceive( MB_VAR_TypeDef *pMbVarStruct, uint8_t * pucRcvAddress, uint8_t ** pucFrame, uint16_t * pusLength )
{
    eMBErrorCode    eStatus = MB_ENOERR;

//    assert( pMbVarStruct->usRcvBufPos < MB_SER_PDU_SIZE_MAX );

    /* Length and CRC check */
    if( ( pMbVarStruct->usRcvBufPos >= MB_SER_PDU_SIZE_MIN ) && ( usMBCRC16( ( uint8_t * ) pMbVarStruct->ucRcvBuffer, pMbVarStruct->usRcvBufPos ) == 0 ) )
    {
        /* Save the address field. All frames are passed to the upper layed
         * and the decision if a frame is used is done there.
         */
        *pucRcvAddress = pMbVarStruct->ucRcvBuffer[MB_SER_PDU_ADDR_OFF];	//从机地址

        /* Total length of Modbus-PDU is Modbus-Serial-Line-PDU minus
         * size of address field and CRC checksum.
         */
        *pusLength = ( uint16_t )( pMbVarStruct->usRcvBufPos - MB_SER_PDU_PDU_OFF - MB_SER_PDU_SIZE_CRC );	//帧内容长度

        /* Return the start of the Modbus PDU to the caller. */
        *pucFrame = ( uint8_t * ) & pMbVarStruct->ucRcvBuffer[MB_SER_PDU_PDU_OFF]; //帧缓存起始地址
    }
    else
    {
        eStatus = MB_EIO;
    }

    return eStatus;
}

/*
* 完整帧，内容分段（从机地址 + 帧内容 + 帧长度），接收数据缓存，同时校验位核定
* 问题，关中断，完整帧解析过程中，数据丢失，影响其他功能
  解决方法: uint8_t xMBRTUReceiveFSM( MB_VAR_TypeDef *pMbVarStruct )
						标记帧结束POS;	case STATE_RX_IDLE:
						采用环形接收中断数据;	case STATE_RX_RCV:
						
*/
eMBErrorCode eMBRTUSend( MB_VAR_TypeDef *pMbVarStruct, uint8_t ucSlaveAddress, const uint8_t * pucFrame, uint16_t usLength )
{
    eMBErrorCode    eStatus = MB_ENOERR;
    uint16_t        usCRC16;

    /* Check if the receiver is still in idle state. If not we where to
     * slow with processing the received frame and the master sent another
     * frame on the network. We have to abort sending the frame.
     */
    if( pMbVarStruct->eRcvState == STATE_RX_IDLE )
    {
			/* First byte before the Modbus-PDU is the slave address. */
			pMbVarStruct->pucSndBufferCur = ( uint8_t * ) pucFrame - 1;
			pMbVarStruct->usSndBufferCount = 1;

			/* Now copy the Modbus-PDU into the Modbus-Serial-Line-PDU. */
			pMbVarStruct->pucSndBufferCur[MB_SER_PDU_ADDR_OFF] = ucSlaveAddress;
			pMbVarStruct->usSndBufferCount += usLength;

			/* Calculate CRC16 checksum for Modbus-Serial-Line-PDU. */
			usCRC16 = usMBCRC16( ( uint8_t * ) pMbVarStruct->pucSndBufferCur, pMbVarStruct->usSndBufferCount );
			pMbVarStruct->ucRcvBuffer[pMbVarStruct->usSndBufferCount++] = ( uint8_t )( usCRC16 & 0xFF );
			pMbVarStruct->ucRcvBuffer[pMbVarStruct->usSndBufferCount++] = ( uint8_t )( usCRC16 >> 8 );

			/* Activate the transmitter. */
			pMbVarStruct-> eSndState = STATE_TX_XMIT;
			xMBPortEventPost( pMbVarStruct, TRUE, EV_FRAME_SENT );
    }
    else
    {
      eStatus = MB_EIO;
    }

    return eStatus;
}

/*
* 中断存储，接收数据
*/
uint8_t xMBRTUReceiveFSM( MB_VAR_TypeDef *pMbVarStruct )
{
    uint8_t            xTaskNeedSwitch = FALSE;
    uint8_t           ucByte;

//    assert( pMbVarStruct->eSndState == STATE_TX_IDLE );

    /* Always read the character. */
    ( void )xMBPortSerialGetByte( pMbVarStruct->huart,  &ucByte );
	
    switch ( pMbVarStruct->eRcvState )
    {
        /* If we have received a character in the init state we have to
         * wait until the frame is finished.
         */
    case STATE_RX_INIT:
        vMBPortTimersEnable( pMbVarStruct );
        break;

        /* In the error state we wait until all characters in the
         * damaged frame are transmitted.
         */
    case STATE_RX_ERROR:
        vMBPortTimersEnable( pMbVarStruct );
        break;

        /* In the idle state we wait for a new character. If a character
         * is received the t1.5 and t3.5 timers are started and the
         * receiver is in the state STATE_RX_RECEIVCE.
         */
    case STATE_RX_IDLE:	//新帧标志，存储第一个字节
        pMbVarStruct->usRcvBufPos = 0;
        pMbVarStruct->ucRcvBuffer[pMbVarStruct->usRcvBufPos++] = ucByte;
        pMbVarStruct->eRcvState = STATE_RX_RCV;

        /* Enable t3.5 timers. */
				vMBPortTimersEnable(pMbVarStruct);
        break;

        /* We are currently receiving a frame. Reset the timer after
         * every character received. If more than the maximum possible
         * number of bytes in a modbus frame is received the frame is
         * ignored.
         */
    case STATE_RX_RCV:	//存储帧内容
        if( pMbVarStruct->usRcvBufPos < MB_SER_PDU_SIZE_MAX ){
            pMbVarStruct->ucRcvBuffer[pMbVarStruct->usRcvBufPos++] = ucByte;
        }else{
            pMbVarStruct->eRcvState = STATE_RX_ERROR;
        }
        vMBPortTimersEnable(pMbVarStruct);
        break;
    }
    return xTaskNeedSwitch;
}

/*
* 中断存储，发送数据
*/
uint8_t xMBRTUTransmitFSM( MB_VAR_TypeDef *pMbVarStruct )
{
    uint8_t            xNeedPoll = FALSE;

//    assert( pMbVarStruct->eRcvState == STATE_RX_IDLE );

    switch ( pMbVarStruct->eSndState )
    {
        /* We should not get a transmitter event if the transmitter is in
         * idle state.  */
    case STATE_TX_IDLE:
        /* enable receiver/disable transmitter. */
        vMBPortSerialEnable( pMbVarStruct->huart, TRUE, FALSE );
        break;

    case STATE_TX_XMIT:
        xMBPortSerialPutByte(pMbVarStruct->huart, pMbVarStruct->pucSndBufferCur, (uint16_t)pMbVarStruct->usSndBufferCount);
				pMbVarStruct->eSndState = STATE_TX_IDLE;
/*		
				// check if we are finished. 
        if( pMbVarStruct->usSndBufferCount != 0 )
        {
            xMBPortSerialPutByte( pMbVarStruct->huart, ( int8_t )*pMbVarStruct->pucSndBufferCur );
            pMbVarStruct->pucSndBufferCur++;  // next byte in sendbuffer. 
            pMbVarStruct->usSndBufferCount--;
        }
        else
        {
            xNeedPoll = xMBPortEventPost( pMbVarStruct, TRUE, EV_FRAME_SENT );
            // Disable transmitter. This prevents another transmit buffer empty interrupt.
            vMBPortSerialEnable( pMbVarStruct->huart, TRUE, FALSE );
            pMbVarStruct->eSndState = STATE_TX_IDLE;
        }
*/
        break;
    }

    return xNeedPoll;
}


/*
* 帧分割监测
*/
uint8_t xMBRTUTimerT35Expired( MB_VAR_TypeDef *pMbVarStruct )
{
    uint8_t xNeedPoll = FALSE;

    switch ( pMbVarStruct->eRcvState )
    {
        /* Timer t35 expired. Startup phase is finished. */
		case STATE_RX_INIT:	//上电计时超T35
						xNeedPoll = xMBPortEventPost( pMbVarStruct, FALSE, EV_READY );
						break;

        /* A frame was received and t35 expired. Notify the listener that
         * a new frame was received. */
				case STATE_RX_RCV:	//帧接收结束
						xNeedPoll = xMBPortEventPost( pMbVarStruct, TRUE, EV_FRAME_RECEIVED );
						break;

        /* An error occured while receiving the frame. */
				case STATE_RX_ERROR:
						break;

        /* Function called in an illegal state. */
				default:
//						assert( ( pMbVarStruct->eRcvState == STATE_RX_INIT ) ||
//										( pMbVarStruct->eRcvState == STATE_RX_RCV ) || ( pMbVarStruct->eRcvState == STATE_RX_ERROR ) );
				break;
    }

		vMBPortTimersDisable( pMbVarStruct );
    pMbVarStruct->eRcvState = STATE_RX_IDLE;	//新帧标志变量

    return xNeedPoll;
}
