#include "mb_serial.h"
#include "eth_socket.h"
#include "eth_type.h"
#include "mb_master.h"

#include "string.h"

MB_VAR_PORT g_mb_var_port[CONFIG_MB_PORT_COUNT];

uint8_t txd_frame[CONFIG_MB_TXD_BUFFER_SIZE];

/*
*****************************************************************
* 初始化
*****************************************************************
*/
uint8_t mb_serial_init( uint8_t ucPORT, uint32_t ulBaudRate, uint8_t ucDataBits, eMBParity eParity )
{
	return TRUE;
}

/*
*****************************************************************
* 接收数据
*****************************************************************
*/
void mb_serial_rs485_dir(uint8_t dir)
{
	if(dir == CONFIG_RS485_RECEIVE){
		HAL_GPIO_WritePin(GPIOA, GPIO_PIN_1, GPIO_PIN_RESET);
	}else{
		HAL_GPIO_WritePin(GPIOA, GPIO_PIN_1, GPIO_PIN_SET);
	}
}

/*
*****************************************************************
* 接收数据
*****************************************************************
*/
inline uint8_t mb_serial_receive(UART_HandleTypeDef *huart, uint8_t *buff)
{
	if(HAL_UART_Receive_IT( huart, buff, 1) != HAL_OK ){
		return FALSE;
	}else{
		return TRUE;
	}
}

/*
*****************************************************************
* 发送数据(RTU ASCII TCP)
*****************************************************************
*/
void mb_serial_rs485_send(UART_HandleTypeDef *huart, uint8_t *buf)
{
	uint8_t i, len = strlen((void *)buf);
	
	mb_serial_rs485_dir(CONFIG_RS485_SEND);		//RS485切换方向
	
	for(i = 0; i < len; i++){		//数据拷贝
		txd_frame[i] = buf[i];
	}
	
	HAL_UART_Transmit_IT(huart, txd_frame, len);	//串口输出
}

inline uint8_t mb_serial_send(MB_VAR_PORT *p_mb_var_port)
{
	uint8_t frame_sta = p_mb_var_port->txdframebuf[CONFIG_MB_TXD_FRAME_POS(p_mb_var_port->txdframecnt, -1)];
	uint8_t frame_end = p_mb_var_port->txdframebuf[p_mb_var_port->txdframecnt];
	uint8_t txd_pos, buf_pos = frame_sta;
	
	//主机操作
	if(p_mb_var_port->comm_mode == MB_MASTER){
		if(p_mb_var_port->master_buf_pos == -1){
			p_mb_var_port->master_buf_pos = mb_master_get_buf_id(p_mb_var_port, frame_sta);	
		}
		
		if(p_mb_var_port->master_buf_pos == -1){
			return FALSE;
		}
		
		if((p_mb_var_port->out_time_mark + p_mb_var_port->out_time_value) > HAL_GetTick()){
			return FALSE;
		}
	}
	
	//RS485切换方向
	mb_serial_rs485_dir(CONFIG_RS485_SEND);
	
	//拷贝
	for(txd_pos = 0; ; txd_pos++){
		if(buf_pos == frame_end)
			break;
		txd_frame[txd_pos] = p_mb_var_port->txdbuffer[buf_pos];
		buf_pos = CONFIG_MB_TXD_BUFFER_POS(buf_pos, 1);
	}	
	
	//输出
	if(p_mb_var_port->mode == MB_ASCII || p_mb_var_port->mode == MB_RTU){
		if(HAL_UART_Transmit_IT(p_mb_var_port->huart, txd_frame, txd_pos) != HAL_OK )
			return FALSE ;
	}else if(p_mb_var_port->mode == MB_TCP){
		if(ETH_SOCKET_WriteBuffer(ETH_CONFIG_MODBUS_SOCKET, txd_frame, txd_pos) != txd_pos)
			return FALSE;
	}
	
	if(p_mb_var_port->comm_mode == MB_MASTER)
		p_mb_var_port->out_time_mark = HAL_GetTick();
	else
		mb_serial_txd_frame_delete(p_mb_var_port);
	
	return TRUE;
}

/*
*****************************************************************
* 插入一帧标记
*****************************************************************
*/
inline void mb_serial_txd_frame_insert(MB_VAR_PORT *p_mb_var_port)
{
	p_mb_var_port->txdframebuf[p_mb_var_port->txdframepos] = p_mb_var_port->txdbufpos;
	p_mb_var_port->txdframepos = CONFIG_MB_TXD_FRAME_POS(p_mb_var_port->txdframepos, 1);
}

inline void mb_serial_rxd_frame_insert(MB_VAR_PORT *p_mb_var_port)
{
	p_mb_var_port->rxdframebuf[p_mb_var_port->rxdframepos] = p_mb_var_port->rxdbufpos;
	p_mb_var_port->rxdframepos = CONFIG_MB_RXD_FRAME_POS(p_mb_var_port->rxdframepos, 1);
}

/*
*****************************************************************
* 删除一帧标记
*****************************************************************
*/
inline void mb_serial_txd_frame_delete(MB_VAR_PORT *p_mb_var_port)
{
	p_mb_var_port->txdframecnt = CONFIG_MB_TXD_FRAME_POS(p_mb_var_port->txdframecnt, 1);
}

inline void mb_serial_rxd_frame_delete(MB_VAR_PORT *p_mb_var_port)
{
	p_mb_var_port->rxdframecnt = CONFIG_MB_RXD_FRAME_POS(p_mb_var_port->rxdframecnt, 1);
}
/*
*****************************************************************
* 得一帧基本信息
*****************************************************************
*/
void mb_serial_frame_info(MB_VAR_PORT *p_mb_var_port, uint8_t *p_pos, uint8_t *p_len)
{
	uint8_t end_pos = p_mb_var_port->rxdframebuf[p_mb_var_port->rxdframecnt];
	*p_pos = p_mb_var_port->rxdframebuf[CONFIG_MB_RXD_FRAME_POS(p_mb_var_port->rxdframecnt, -1)];
	
	if(end_pos > *p_pos){
		*p_len = end_pos - *p_pos;
	}else{
		*p_len = (CONFIG_MB_RXD_BUFFER_SIZE - *p_pos) + end_pos;
	}
	
	mb_serial_rxd_frame_delete(p_mb_var_port);
}

