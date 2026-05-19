/*
 * FreeModbus Libary: BARE Port
 * Copyright (C) 2006 Christian Walter <wolti@sil.at>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 * File: $Id: portserial.c,v 1.1 2006/08/22 21:35:13 wolti Exp $
 */
 
 /* ----------------------- Platform includes --------------------------------*/
#include "mb_type.h"

/* If xRXEnable enable serial receive interrupts. If xTxENable enable
	 * transmitter empty interrupts.
	 */
void vMBPortSerialEnable( UART_HandleTypeDef *huart, uint8_t xRxEnable, uint8_t xTxEnable )
{
	if (xRxEnable){
		__HAL_UART_ENABLE_IT(huart, UART_IT_RXNE);
	}else{
		__HAL_UART_DISABLE_IT(huart, UART_IT_RXNE);
	}
	
	if (xTxEnable){
		__HAL_UART_ENABLE_IT(huart, UART_IT_TXE);
	}else{
		__HAL_UART_DISABLE_IT(huart, UART_IT_TXE);
	}
}

uint8_t xMBPortSerialInit( uint8_t ucPORT, uint32_t ulBaudRate, uint8_t ucDataBits, eMBParity eParity )
{
	return TRUE;
}


/* Put a byte in the UARTs transmit buffer. This function is called
 * by the protocol stack if pxMBFrameCBTransmitterEmpty( ) has been
 * called. 
*/
uint8_t xMBPortSerialPutByte( UART_HandleTypeDef *huart, uint8_t *pData, uint16_t size )
{
	if(HAL_UART_Transmit_IT(huart, pData, size) != HAL_OK )
		return FALSE ;
	else
		return TRUE;
}

/* Return the byte in the UARTs receive buffer. This function is called
 * by the protocol stack after pxMBFrameCBByteReceived( ) has been called.
 */
uint8_t xMBPortSerialGetByte( UART_HandleTypeDef *huart, uint8_t *pucByte )
{
	if(HAL_UART_Receive_IT ( huart, pucByte, 1) != HAL_OK )
		return FALSE;
	else
		return TRUE;
}

