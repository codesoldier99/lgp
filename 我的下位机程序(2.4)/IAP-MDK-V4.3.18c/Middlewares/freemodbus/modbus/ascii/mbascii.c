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
 * File: $Id: mbascii.c,v 1.17 2010/06/06 13:47:07 wolti Exp $
 */

/* ----------------------- System includes ----------------------------------*/
#include "stdlib.h"
#include "string.h"
#include <assert.h>

/* ----------------------- Modbus includes ----------------------------------*/
#include "mb.h"
#include "mbconfig.h"
#include "mbascii.h"
#include "mbframe.h"

#include "mbcrc.h"
#include "mbport.h"

#if MB_ASCII_ENABLED > 0

/* ----------------------- Defines ------------------------------------------*/
#define MB_ASCII_DEFAULT_CR     '\r'    /*!< Default CR character for Modbus ASCII. */
#define MB_ASCII_DEFAULT_LF     '\n'    /*!< Default LF character for Modbus ASCII. */
#define MB_SER_PDU_SIZE_MIN     3       /*!< Minimum size of a Modbus ASCII frame. */
#define MB_SER_PDU_SIZE_MAX     64     /*!< Maximum size of a Modbus ASCII frame. */
#define MB_SER_PDU_SIZE_LRC     1       /*!< Size of LRC field in PDU. */
#define MB_SER_PDU_ADDR_OFF     0       /*!< Offset of slave address in Ser-PDU. */
#define MB_SER_PDU_PDU_OFF      1       /*!< Offset of Modbus-PDU in Ser-PDU. */

/* ----------------------- Static functions ---------------------------------*/
static uint8_t    prvucMBCHAR2BIN( uint8_t ucCharacter );

static uint8_t    prvucMBBIN2CHAR( uint8_t ucByte );

static uint8_t    prvucMBLRC( uint8_t * pucFrame, uint16_t usLen );

/* We reuse the Modbus RTU buffer because only one buffer is needed and the
 * RTU buffer is bigger. */
static volatile eMBBytePos eBytePos;

static volatile uint8_t ucLRC;
static uint8_t ucMBLFCharacter;

/* ----------------------- Start implementation -----------------------------*/
eMBErrorCode eMBASCIIInit(MB_VAR_TypeDef *pMbVarStruct, uint8_t ucSlaveAddress, uint8_t ucPort, uint32_t ulBaudRate, eMBParity eParity)
{
    eMBErrorCode    eStatus = MB_ENOERR;
    ( void )ucSlaveAddress;
    
    ucMBLFCharacter = MB_ASCII_DEFAULT_LF;

    if( xMBPortSerialInit( ucPort, ulBaudRate, 7, eParity ) != TRUE ){
        eStatus = MB_EPORTERR;
    }
    else{ 
			pMbVarStruct->timer_value = MB_ASCII_TIMEOUT_SEC * 1000UL;	
    }

    return eStatus;
}

void eMBASCIIStart( MB_VAR_TypeDef *pMbVarStruct )
{
    vMBPortSerialEnable( pMbVarStruct->huart, TRUE, FALSE );
    pMbVarStruct->eRcvState = STATE_RX_IDLE;

    /* No special startup required for ASCII. */
    ( void )xMBPortEventPost( pMbVarStruct, FALSE, EV_READY );
}

void eMBASCIIStop( MB_VAR_TypeDef *pMbVarStruct )
{
    vMBPortSerialEnable( pMbVarStruct->huart, FALSE, FALSE );
    vMBPortTimersDisable( pMbVarStruct );
}

eMBErrorCode eMBASCIIReceive( MB_VAR_TypeDef *pMbVarStruct, uint8_t * pucRcvAddress, uint8_t ** pucFrame, uint16_t * pusLength )
{
    eMBErrorCode    eStatus = MB_ENOERR;

    assert( pMbVarStruct->usRcvBufPos < MB_SER_PDU_SIZE_MAX );

    /* Length and CRC check */
    if( ( pMbVarStruct->usRcvBufPos >= MB_SER_PDU_SIZE_MIN )
        && ( prvucMBLRC( ( uint8_t * ) pMbVarStruct->ucRcvBuffer, pMbVarStruct->usRcvBufPos ) == 0 ) )
    {
        /* Save the address field. All frames are passed to the upper layed
         * and the decision if a frame is used is done there.
         */
        *pucRcvAddress = pMbVarStruct->ucRcvBuffer[MB_SER_PDU_ADDR_OFF];

        /* Total length of Modbus-PDU is Modbus-Serial-Line-PDU minus
         * size of address field and CRC checksum.
         */
        *pusLength = ( uint16_t )( pMbVarStruct->usRcvBufPos - MB_SER_PDU_PDU_OFF - MB_SER_PDU_SIZE_LRC );

        /* Return the start of the Modbus PDU to the caller. */
        *pucFrame = ( uint8_t * ) & pMbVarStruct->ucRcvBuffer[MB_SER_PDU_PDU_OFF];
    }
    else
    {
        eStatus = MB_EIO;
    }

    return eStatus;
}

eMBErrorCode eMBASCIISend( MB_VAR_TypeDef *pMbVarStruct, uint8_t ucSlaveAddress, const uint8_t * pucFrame, uint16_t usLength )
{
    eMBErrorCode    eStatus = MB_ENOERR;
    uint8_t           usLRC;

    /* Check if the receiver is still in idle state. If not we where too
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

        /* Calculate LRC checksum for Modbus-Serial-Line-PDU. */
        usLRC = prvucMBLRC( ( uint8_t * ) pMbVarStruct->pucSndBufferCur, pMbVarStruct->usSndBufferCount );
        pMbVarStruct->ucRcvBuffer[pMbVarStruct->usSndBufferCount++] = usLRC;

        /* Activate the transmitter. */
        pMbVarStruct->eSndState = STATE_TX_START;
        vMBPortSerialEnable( pMbVarStruct->huart, FALSE, TRUE );
    }
    else
    {
        eStatus = MB_EIO;
    }
    return eStatus;
}

uint8_t xMBASCIIReceiveFSM( MB_VAR_TypeDef *pMbVarStruct )
{
    uint8_t            xNeedPoll = FALSE;
    uint8_t           ucByte;
    uint8_t           ucResult;

    assert( pMbVarStruct->eSndState == STATE_TX_IDLE );

    ( void )xMBPortSerialGetByte( pMbVarStruct->huart, & ucByte );
    switch ( pMbVarStruct->eRcvState )
    {
        /* A new character is received. If the character is a ':' the input
         * buffer is cleared. A CR-character signals the end of the data
         * block. Other characters are part of the data block and their
         * ASCII value is converted back to a binary representation.
         */
    case STATE_RX_RCV:
        /* Enable timer for character timeout. */
        vMBPortTimersEnable( pMbVarStruct );
        if( ucByte == ':' ){
            /* Empty receive buffer. */
            eBytePos = BYTE_HIGH_NIBBLE;
            pMbVarStruct->usRcvBufPos = 0;
        }else if( ucByte == MB_ASCII_DEFAULT_CR ){
            pMbVarStruct->eRcvState = STATE_RX_WAIT_EOF;
        }else{
            ucResult = prvucMBCHAR2BIN( ucByte );
            switch ( eBytePos ){
                /* High nibble of the byte comes first. We check for a buffer overflow here. */
							case BYTE_HIGH_NIBBLE:
                if( pMbVarStruct->usRcvBufPos < MB_SER_PDU_SIZE_MAX ){
                    pMbVarStruct->ucRcvBuffer[pMbVarStruct->usRcvBufPos] = ( uint8_t )( ucResult << 4 );
                    eBytePos = BYTE_LOW_NIBBLE;
                    break;
                }else{
                    /* not handled in Modbus specification but seems a resonable implementation. */
                    pMbVarStruct->eRcvState = STATE_RX_IDLE;
                    /* Disable previously activated timer because of error state. */
                    vMBPortTimersDisable( pMbVarStruct );
                }
                break;

							case BYTE_LOW_NIBBLE:
                pMbVarStruct->ucRcvBuffer[pMbVarStruct->usRcvBufPos] |= ucResult;
                pMbVarStruct->usRcvBufPos++;
                eBytePos = BYTE_HIGH_NIBBLE;
                break;
            }
        }
        break;

    case STATE_RX_WAIT_EOF:
        if( ucByte == ucMBLFCharacter )
        {
            /* Disable character timeout timer because all characters are received. */
            vMBPortTimersDisable( pMbVarStruct );
            /* Receiver is again in idle state. */
            pMbVarStruct->eRcvState = STATE_RX_IDLE;

            /* Notify the caller of eMBASCIIReceive that a new frame was received. */
            xNeedPoll = xMBPortEventPost( pMbVarStruct, TRUE, EV_FRAME_RECEIVED );
        }
        else if( ucByte == ':' )
        {
            /* Empty receive buffer and back to receive state. */
            eBytePos = BYTE_HIGH_NIBBLE;
            pMbVarStruct->usRcvBufPos = 0;
            pMbVarStruct->eRcvState = STATE_RX_RCV;

            /* Enable timer for character timeout. */
            vMBPortTimersEnable( pMbVarStruct );
        }
        else
        {
            /* Frame is not okay. Delete entire frame. */
            pMbVarStruct->eRcvState = STATE_RX_IDLE;
        }
        break;

    case STATE_RX_IDLE:
        if( ucByte == ':' )
        {
            /* Enable timer for character timeout. */
            vMBPortTimersEnable( pMbVarStruct );
            /* Reset the input buffers to store the frame. */
            pMbVarStruct->usRcvBufPos = 0;;
            eBytePos = BYTE_HIGH_NIBBLE;
            pMbVarStruct->eRcvState = STATE_RX_RCV;
        }
        break;
				
		case STATE_RX_INIT:
			break;
		
		case STATE_RX_ERROR:
			break;
				
    }

    return xNeedPoll;
}

uint8_t xMBASCIITransmitFSM( MB_VAR_TypeDef *pMbVarStruct )
{
    uint8_t            xNeedPoll = FALSE;
    uint8_t           ucByte;

    assert( pMbVarStruct->eRcvState == STATE_RX_IDLE );
    switch ( pMbVarStruct->eSndState )
    {
        /* Start of transmission. The start of a frame is defined by sending
         * the character ':'. */
    case STATE_TX_START:
        ucByte = ':';
        xMBPortSerialPutByte( pMbVarStruct->huart, &ucByte, 1 );
        pMbVarStruct->eSndState = STATE_TX_DATA;
        eBytePos = BYTE_HIGH_NIBBLE;
        break;

        /* Send the data block. Each data byte is encoded as a character hex
         * stream with the high nibble sent first and the low nibble sent
         * last. If all data bytes are exhausted we send a '\r' character
         * to end the transmission. */
    case STATE_TX_DATA:
        if( pMbVarStruct->usSndBufferCount > 0 )
        {
            switch ( eBytePos )
            {
            case BYTE_HIGH_NIBBLE:
                ucByte = prvucMBBIN2CHAR( ( uint8_t )( *pMbVarStruct->pucSndBufferCur >> 4 ) );
                xMBPortSerialPutByte( pMbVarStruct->huart, &ucByte, 1 );
                eBytePos = BYTE_LOW_NIBBLE;
                break;

            case BYTE_LOW_NIBBLE:
                ucByte = prvucMBBIN2CHAR( ( uint8_t )( *pMbVarStruct->pucSndBufferCur & 0x0F ) );
                xMBPortSerialPutByte( pMbVarStruct->huart, &ucByte, 1 );
                pMbVarStruct->pucSndBufferCur++;
                eBytePos = BYTE_HIGH_NIBBLE;
                pMbVarStruct->usSndBufferCount--;
                break;
            }
        }
        else
        {
            ucByte = MB_ASCII_DEFAULT_CR;
						xMBPortSerialPutByte( pMbVarStruct->huart, &ucByte, 1 );
            pMbVarStruct->eSndState = STATE_TX_END;
        }
        break;

        /* Finish the frame by sending a LF character. */
    case STATE_TX_END:
        xMBPortSerialPutByte( pMbVarStruct->huart, &ucMBLFCharacter, 1);
        /* We need another state to make sure that the CR character has
         * been sent. */
        pMbVarStruct->eSndState = STATE_TX_NOTIFY;
        break;

        /* Notify the task which called eMBASCIISend that the frame has
         * been sent. */
    case STATE_TX_NOTIFY:
        pMbVarStruct->eSndState = STATE_TX_IDLE;
        xNeedPoll = xMBPortEventPost( pMbVarStruct, TRUE, EV_FRAME_SENT );

        /* Disable transmitter. This prevents another transmit buffer
         * empty interrupt. */
        vMBPortSerialEnable( pMbVarStruct->huart, TRUE, FALSE );
        pMbVarStruct->eSndState = STATE_TX_IDLE;
        break;

        /* We should not get a transmitter event if the transmitter is in
         * idle state.  */
    case STATE_TX_IDLE:
        /* enable receiver/disable transmitter. */
        vMBPortSerialEnable( pMbVarStruct->huart, TRUE, FALSE );
        break;
		
		case STATE_TX_XMIT:
			break;
    }

    return xNeedPoll;
}

uint8_t xMBASCIITimerT1SExpired( MB_VAR_TypeDef *pMbVarStruct )
{
    switch ( pMbVarStruct->eRcvState )
    {
        /* If we have a timeout we go back to the idle state and wait for
         * the next frame.
         */
    case STATE_RX_RCV:
    case STATE_RX_WAIT_EOF:
        pMbVarStruct->eRcvState = STATE_RX_IDLE;
        break;

    default:
        assert( ( pMbVarStruct->eRcvState == STATE_RX_RCV ) || ( pMbVarStruct->eRcvState == STATE_RX_WAIT_EOF ) );
        break;
    }
    vMBPortTimersDisable( pMbVarStruct );

    /* no context switch required. */
    return FALSE;
}


static          uint8_t
prvucMBCHAR2BIN( uint8_t ucCharacter )
{
    if( ( ucCharacter >= '0' ) && ( ucCharacter <= '9' ) )
    {
        return ( uint8_t )( ucCharacter - '0' );
    }
    else if( ( ucCharacter >= 'A' ) && ( ucCharacter <= 'F' ) )
    {
        return ( uint8_t )( ucCharacter - 'A' + 0x0A );
    }
    else
    {
        return 0xFF;
    }
}

static          uint8_t
prvucMBBIN2CHAR( uint8_t ucByte )
{
    if( ucByte <= 0x09 )
    {
        return ( uint8_t )( '0' + ucByte );
    }
    else if( ( ucByte >= 0x0A ) && ( ucByte <= 0x0F ) )
    {
        return ( uint8_t )( ucByte - 0x0A + 'A' );
    }
    else
    {
        /* Programming error. */
        assert( 0 );
    }
    return '0';
}


static          uint8_t
prvucMBLRC( uint8_t * pucFrame, uint16_t usLen )
{
    uint8_t           ucLRC = 0;  /* LRC char initialized */

    while( usLen-- )
    {
        ucLRC += *pucFrame++;   /* Add buffer byte without carry */
    }

    /* Return twos complement */
    ucLRC = ( uint8_t ) ( -( ( int8_t ) ucLRC ) );
    return ucLRC;
}

#endif
