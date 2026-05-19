/*
* FreeModbus Libary: lwIP Port
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
* File: $Id: porttcp.c,v 1.1 2006/08/30 23:18:07 wolti Exp $
*/

/* ----------------------- System includes ----------------------------------*/
#include <stdio.h>
#include <string.h>

#include "mbport.h"

#include "eth_type.h"
#include "eth_socket.h"

/* ----------------------- MBAP Header --------------------------------------*/
#define MB_TCP_UID          6
#define MB_TCP_LEN          4
#define MB_TCP_FUNC         7

/* ----------------------- Prototypes ---------------------------------------*/
uint8_t xMBTCPPortInit( uint16_t usTCPPort )
{	 
	return TRUE;
}


void vMBTCPPortClose( MB_VAR_TypeDef *pMbVarStruct )
{
	
}

void vMBTCPPortDisable( void )
{
	   
}

uint8_t xMBTCPPortGetRequest( MB_VAR_TypeDef *pMbVarStruct, uint8_t ** ppucMBTCPFrame, uint16_t * usTCPLength )
{
	*ppucMBTCPFrame = pMbVarStruct->ucRcvBuffer;
	*usTCPLength    = pMbVarStruct->usRcvBufPos;
	
	return TRUE;
}

uint8_t xMBTCPPortSendResponse(MB_VAR_TypeDef *pMbVarStruct, const uint8_t * pucMBTCPFrame, uint16_t usTCPLength )
{ 
	ETH_SOCKET_WriteBuffer(ETH_CONFIG_MODBUS_SOCKET, (void *)pucMBTCPFrame, usTCPLength);
	return TRUE;
}

uint8_t xMBTCPRecv( MB_VAR_TypeDef *pMbVarStruct )
{  
	pMbVarStruct->usRcvBufPos = ETH_SOCKET_ReadBuffer(ETH_CONFIG_MODBUS_SOCKET, pMbVarStruct->ucRcvBuffer);
	
	if(pMbVarStruct->usRcvBufPos == 0)
		return FALSE;

	(void)xMBPortEventPost( pMbVarStruct, TRUE, EV_FRAME_RECEIVED );
	
	return TRUE;
}

void EnterCriticalSection( void )
{
  __disable_irq();
}

void ExitCriticalSection( void )
{
  __enable_irq();
}
