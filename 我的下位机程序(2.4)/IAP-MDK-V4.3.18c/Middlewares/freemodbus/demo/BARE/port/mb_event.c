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
 * File: $Id: portevent.c,v 1.1 2006/08/22 21:35:13 wolti Exp $
 */

#include "mbport.h"

/* ----------------------- Start implementation -----------------------------*/
uint8_t xMBPortEventInit( MB_VAR_TypeDef *pMbVarStruct )
{
	pMbVarStruct->xEventInQueue = FALSE;
	return TRUE;
}

/*
 eQueuedEvent 状态变量赋值
typedef enum
{
    EV_READY,                   //!< Startup finished.
    EV_FRAME_RECEIVED,          //!< Frame received.
    EV_EXECUTE,                 //!< Execute function.
    EV_FRAME_SENT               //< Frame sent.
} eMBEventType;
*/
uint8_t xMBPortEventPost( MB_VAR_TypeDef *pMbVarStruct, uint8_t ucQueue, eMBEventType eEvent )
{
	pMbVarStruct->xEventInQueue = ucQueue;
	pMbVarStruct->eQueuedEvent = eEvent;
	return TRUE;
}

/*
被 eMBErrorCode eMBPoll( void ) 调用 mb.c
*/
uint8_t xMBPortEventGet( MB_VAR_TypeDef *pMbVarStruct, eMBEventType * eEvent )
{
	uint8_t xEventHappened = FALSE;

	if( pMbVarStruct->xEventInQueue )
	{
		*eEvent = pMbVarStruct->eQueuedEvent;
		pMbVarStruct->xEventInQueue = FALSE;
	
		xEventHappened = TRUE;
	}
	return xEventHappened;
}

