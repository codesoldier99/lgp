#include "app_usart.h"


#include "stm32f1xx_hal.h"

#include "main.h"

#include "mb_type.h"
#include "mb_timer.h"
#include "mb_serial.h"

uint8_t buffer;

void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart)
{
	HAL_UART_Receive_IT(huart, &buffer, 1);
	
	if(huart->Instance == USART1){
		g_mb_var_port[CONFIG_MB_PORT_USART1].mb_receive(CONFIG_MB_PORT_USART1, buffer);
	}else if(huart->Instance == USART2){
		g_mb_var_port[CONFIG_MB_PORT_USART2].mb_receive(CONFIG_MB_PORT_USART2, buffer);
	}else if(huart->Instance == USART3){
		g_mb_var_port[CONFIG_MB_PORT_USART3].mb_receive(CONFIG_MB_PORT_USART3, buffer);
	}else if(huart->Instance == UART4){
		g_mb_var_port[CONFIG_MB_PORT_UART4].mb_receive(CONFIG_MB_PORT_UART4, buffer);
	}else if(huart->Instance == UART5){
		g_mb_var_port[CONFIG_MB_PORT_UART5].mb_receive(CONFIG_MB_PORT_UART5, buffer);
	}
}

void HAL_UART_TxCpltCallback(UART_HandleTypeDef *huart)
{
	if(huart->Instance == USART1){
		mb_timer_snd_update(&g_mb_var_port[CONFIG_MB_PORT_USART1]);
	}else if(huart->Instance == USART2){
		mb_timer_snd_update(&g_mb_var_port[CONFIG_MB_PORT_USART2]);
		mb_serial_rs485_dir(CONFIG_RS485_RECEIVE);
	}else if(huart->Instance == USART3){
		mb_timer_snd_update(&g_mb_var_port[CONFIG_MB_PORT_USART3]);
	}else if(huart->Instance == UART4){
		mb_timer_snd_update(&g_mb_var_port[CONFIG_MB_PORT_UART4]);
	}else if(huart->Instance == UART5){
		mb_timer_snd_update(&g_mb_var_port[CONFIG_MB_PORT_UART5]);
	}
}
