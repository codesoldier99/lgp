/*
 * FreeModbus Libary: BARE Demo Application
 * Copyright (C) 2006 Christian Walter <wolti@sil.at>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 * File: $Id: demo.c,v 1.1 2006/08/22 21:35:13 wolti Exp $
 */

/* ----------------------- Modbus includes ----------------------------------*/

#include <string.h>

#include "app_type.h"

#include "mb_user.h"
#include "mbconfig.h"
#include "mb.h"

#include "eth_type.h"
#include "eth_w5500.h"
#include "eth_socket.h"
#include "eth_spi.h"

MB_PORT_TypeDef MB_PORT_Struct[MODBUS_PORT_COUNT];

/* ----------------------- Defines ------------------------------------------*/
#define REG_INPUT_START 30001
#define REG_INPUT_NREGS 4
static uint16_t usRegInputBuf[REG_INPUT_NREGS];


/* ----------------------- Static variables ---------------------------------*/
#define REG_HOLDING_START 40001
static uint16_t usRegHoldingBufSize = APP_USER_COMM_BUF_SIZE;
static uint8_t *pusRegHoldingBuf = &usAppUserCommBuf[0][0];

/* ----------------------- Start implementation -----------------------------*/

eMBErrorCode eMBRegInputCB( uint8_t * pucRegBuffer, uint16_t usAddress, uint16_t usNRegs )
{
    eMBErrorCode    eStatus = MB_ENOERR;
    int             iRegIndex;

    if( ( usAddress >= REG_INPUT_START ) && ( usAddress + usNRegs <= REG_INPUT_START + REG_INPUT_NREGS ) )
    {
			iRegIndex = ( int )( usAddress - REG_INPUT_START) * 2;
			while( usNRegs > 0 ){
				*pucRegBuffer++ = ( unsigned char )( usRegInputBuf[iRegIndex + 1] );
				*pucRegBuffer++ = ( unsigned char )( usRegInputBuf[iRegIndex + 0] );
				iRegIndex += 2;
				usNRegs--;
			}
    }
    else
    {
			eStatus = MB_ENOREG;
    }

    return eStatus;
}

/*
保持寄存器

@pucRegBuffer	DATA起始地址
@usAddress		寄存器地址
@usNRegs			DATA个数
@eMode				操作模式
uint8_t * pucRegBuffer, uint
*/
eMBErrorCode eMBRegHoldingCB( MB_VAR_TypeDef *p_var_struct, uint16_t usAddress, uint16_t usNRegs, eMBRegisterMode eMode )
{
  eMBErrorCode    eStatus = MB_ENOERR;
	int             iRegIndex;

	if( ( usAddress >= REG_HOLDING_START ) && ( usAddress + usNRegs <= REG_HOLDING_START + usRegHoldingBufSize ) )
	{
			iRegIndex = ( int )( usAddress - REG_HOLDING_START ) * 2;
			switch ( eMode )
			{
				// Pass current register values to the protocol stack. 
				case MB_REG_READ:
					while( usNRegs > 0 ){
							p_var_struct->ucTxdBufPos++;
							p_var_struct->ucTxdBuffer[p_var_struct->ucTxdBufPos] = ( uint8_t ) ( pusRegHoldingBuf[iRegIndex + 1] );
							p_var_struct->ucTxdBufPos++;
							p_var_struct->ucTxdBuffer[p_var_struct->ucTxdBufPos] = ( uint8_t ) ( pusRegHoldingBuf[iRegIndex + 0] );
							iRegIndex += 2;
							usNRegs--;
					}
				break;

				// Update current register values with new values from the protocol stack.
				case MB_REG_WRITE:
					while( usNRegs > 0 ){
							p_var_struct->usRcvBufCnt++;
							pusRegHoldingBuf[iRegIndex + 1] = p_var_struct->ucRcvBuffer[p_var_struct->usRcvBufCnt];
							p_var_struct->usRcvBufCnt++;
							pusRegHoldingBuf[iRegIndex + 0] = p_var_struct->ucRcvBuffer[p_var_struct->usRcvBufCnt];
							iRegIndex += 2;
							usNRegs--;
					}
				break;
			}
	}
	else
	{
			eStatus = MB_ENOREG;
	}
	return eStatus;
}


eMBErrorCode eMBRegCoilsCB( uint8_t * pucRegBuffer, uint16_t usAddress, uint16_t usNCoils, eMBRegisterMode eMode )
{
    return MB_ENOREG;
}

eMBErrorCode eMBRegDiscreteCB( uint8_t * pucRegBuffer, uint16_t usAddress, uint16_t usNDiscrete )
{
    return MB_ENOREG;
}

/**
* 串口初始化
*
*/
eMBErrorCode MB_USER_SerialInit( MB_PORT_TypeDef *pMbPortStruct, eMBMode eMode, UART_HandleTypeDef *huart, uint32_t ulBaud)
{
    eMBErrorCode    eStatus;

		/* 端口配置 */
		pMbPortStruct->var_struct.huart = huart;
	
		/* modbus初始化 */
    eStatus = eMBInit( pMbPortStruct, eMode, 0x01, 0, ulBaud, MB_PAR_EVEN );
    
		/* modbus协议栈 */
    eStatus = eMBEnable( pMbPortStruct );
		
		return eStatus;
}

/**
* TCP初始化
*
*/
eMBErrorCode MB_USER_TcpInit( MB_PORT_TypeDef *pPortStruct, uint16_t ucTCPPort)
{
	eMBErrorCode    eStatus;
	
	/* modbus初始化 */
	eStatus = eMBTCPInit( pPortStruct, ucTCPPort );
	
	/* modbus协议栈 */
  eStatus = eMBEnable( pPortStruct );
	
	return eStatus;
}


/**
* modbus协议
*/
/*
void MB_USER_Task(void *pvParameter)
{
	// tcp
	#if	MB_PORT_TCP_ENABLED > 0
		uint8_t  eth_status = 0;
	  uint8_t	 modbus_tcp_tx_ready = FALSE;
		uint8_t  eth_listen_state = FALSE;
	  uint16_t eth_len = 0;
	  uint8_t  eth_buff[128];
	
		// 硬件复位
		ETH_SPI_SetRst(LOW);
		vTaskDelay(50);
		ETH_SPI_SetRst(HIGH);
		vTaskDelay(200);	
		
		// 网络配置
		ETH_W5500_LocalVarConfig();
		ETH_SOCKET_RemoteVarConfig(ETH_CONFIG_MODBUS_SOCKET, ETH_CONFIG_MODBUS_LOCAL_PORT, (void *)ETH_CONFIG_MODBUS_IP, ETH_CONFIG_MODBUS_REMOTE_PORT);	
	#endif
	
	// usart1
	#if MB_PORT_UART1_ENABLED > 0
		MB_USER_SerialInit( &MB_PORT_Struct[MODBUS_PORT_UART1], MB_RTU, &huart1, 115200);
		HAL_UART_Receive_IT(&huart1, MB_PORT_Struct[MODBUS_PORT_UART1].var_struct.ucRcvBuffer, 1);
		HAL_TIM_Base_Start_IT(&htim6);
	#endif
	
	// usart6
	#if MB_PORT_UART6_ENABLED > 0
		MB_USER_SerialInit( &MB_PORT_Struct[MODBUS_PORT_UART6], MB_RTU, &huart6, 115200);
		HAL_UART_Receive_IT(&huart6, MB_PORT_Struct[MODBUS_PORT_UART6].var_struct.ucRcvBuffer, 1);
		HAL_TIM_Base_Start_IT(&htim7);
	#endif
	
	#if	MB_PORT_TCP_ENABLED > 0
		MB_USER_TcpInit( &MB_PORT_Struct[MODBUS_PORT_TCP], 502);
	#endif
	
	for(;;)
	{
		vTaskDelay(10);
		//[MODBUS-TCP]
		#if	MB_PORT_TCP_ENABLED > 0
			//1. 网络状态
			eth_status = ETH_SOCKET_IrStatus(ETH_CONFIG_MODBUS_SOCKET);
			if( eth_status & SOCKET_IR_DISCON || eth_listen_state == FALSE){
				// 配置网络
				ETH_W5500_BaseRegConfig();
				ETH_W5500_LocalRegConfig();
				ETH_SOCKET_RemoteRegConfig(ETH_CONFIG_MODBUS_SOCKET);

				// 设置侦听模式
				for(;;){
					if(ETH_SOCKET_TcpListen(ETH_CONFIG_MODBUS_SOCKET) == TRUE){
						vTaskDelay(10);
						break;
					}
				}
				
				eth_listen_state = TRUE;
			}
				
			// 连接状态
			if(ETH_SOCKET_SrStatus(ETH_CONFIG_MODBUS_SOCKET) == SOCK_ESTABLISHED){
				//2. 发送一帧数据
//					if(eth_status & SOCKET_IR_SEND){
//						modbus_tcp_tx_ready = TRUE;
//					}
//					if(modbus_tcp_tx_ready == TRUE){
//						ETH_SOCKET_WriteBuffer(ETH_CONFIG_MODBUS_SOCKET, (void *)"welcom to modbus tcp\r\n", 22);
//						modbus_tcp_tx_ready = FALSE;
//					}
				
				//3. 接收一帧数据
				if(eth_status & SOCKET_IR_RECV){
					xMBTCPRecv( &MB_PORT_Struct[MODBUS_PORT_TCP].var_struct );
//						eth_len = ETH_SOCKET_ReadBuffer(ETH_CONFIG_MODBUS_SOCKET, eth_buff);
				}
			}
				
			eMBPoll( &MB_PORT_Struct[MODBUS_PORT_TCP] );
		#endif
		
//[MODBUS-RTU]
		#if MB_PORT_UART1_ENABLED > 0
			eMBPoll( &MB_PORT_Struct[MODBUS_PORT_UART1] );
			//HAL_UART_Transmit_IT(&huart1, (void *)"welcom to usart1\r\n", 18);
		#endif
		
//[MODBUS-RTU]
		#if MB_PORT_UART6_ENABLED > 0
			eMBPoll( &MB_PORT_Struct[MODBUS_PORT_UART6] );
		//	HAL_UART_Transmit_IT(&huart6, (void *)"welcom to usart6\r\n", 18);
		#endif
	}
}*/



