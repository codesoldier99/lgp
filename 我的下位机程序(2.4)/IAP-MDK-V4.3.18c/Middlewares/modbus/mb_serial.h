#ifndef __MB_SERIAL_H__
#define __MB_SERIAL_H__

#include "mb_type.h"

// 初始化
uint8_t mb_serial_init( uint8_t ucPORT, uint32_t ulBaudRate, uint8_t ucDataBits, eMBParity eParity );

// RS485方向
void mb_serial_rs485_dir(uint8_t dir);
	
// 接收数据
uint8_t mb_serial_receive(UART_HandleTypeDef *huart, uint8_t *buff);

// 发送数据
void mb_serial_rs485_send(UART_HandleTypeDef *huart, uint8_t *buf);
uint8_t mb_serial_send(MB_VAR_PORT *p_mb_var_port);

// 插入一帧标记
void mb_serial_txd_frame_insert(MB_VAR_PORT *p_mb_var_port);
void mb_serial_rxd_frame_insert(MB_VAR_PORT *p_mb_var_port);

// 删除一帧标记
void mb_serial_txd_frame_delete(MB_VAR_PORT *p_mb_var_port);
void mb_serial_rxd_frame_delete(MB_VAR_PORT *p_mb_var_port);

// 得一帧基本信息
void mb_serial_frame_info(MB_VAR_PORT *p_mb_var_port, uint8_t *p_pos, uint8_t *p_len);

#endif

