#include "mb_user.h"
#include "mb_tcp.h"

static uint8_t poll_id = 0;


/*
*****************************************************************
* 端口初始化
*****************************************************************
*/
void mb_user_init(void)
{
	MB_VAR_PORT *p_mb_var_port;
	uint8_t      i = 0;
	/*
*****************************************************************
* 端口串口指针
*****************************************************************
*/
	p_mb_var_port        = g_mb_var_port + CONFIG_MB_PORT_USART1;
  p_mb_var_port->huart = &huart1;
	mb_port_init(CONFIG_MB_PORT_USART1, MB_SLAVE, MB_RTU, 1, 502, 115200, MB_PAR_NONE);
	HAL_UART_Receive_IT(p_mb_var_port->huart, &p_mb_var_port->rxdbuffer[p_mb_var_port->rxdbufpos], 1);
	for(i = 0; i < CONFIG_MB_VAR_MASTER_BUF_SIZE; i++){
		p_mb_var_port->mb_var_master_buf[i].pos = CONFIG_MASTER_POS_FREE;
	}
	p_mb_var_port->master_buf_pos = -1;
	
	p_mb_var_port        = g_mb_var_port + CONFIG_MB_PORT_USART2;
	p_mb_var_port->huart = &huart2;
	mb_port_init(CONFIG_MB_PORT_USART2, MB_MASTER, MB_RTU, 1, 502, 9600, MB_PAR_NONE);
	HAL_UART_Receive_IT(p_mb_var_port->huart, &p_mb_var_port->rxdbuffer[p_mb_var_port->rxdbufpos], 1);
	for(i = 0; i < CONFIG_MB_VAR_MASTER_BUF_SIZE; i++){
		p_mb_var_port->mb_var_master_buf[i].pos = CONFIG_MASTER_POS_FREE;
	}
	p_mb_var_port->master_buf_pos = -1;
	
	p_mb_var_port        = g_mb_var_port + CONFIG_MB_PORT_USART3;
	p_mb_var_port->huart = &huart3;
	mb_port_init(CONFIG_MB_PORT_USART3, MB_SLAVE, MB_RTU, 1, 502, 115200, MB_PAR_NONE);
	HAL_UART_Receive_IT(p_mb_var_port->huart, &p_mb_var_port->rxdbuffer[p_mb_var_port->rxdbufpos], 1);
	for(i = 0; i < CONFIG_MB_VAR_MASTER_BUF_SIZE; i++){
		p_mb_var_port->mb_var_master_buf[i].pos = CONFIG_MASTER_POS_FREE;
	}
	p_mb_var_port->master_buf_pos = -1;
	
	p_mb_var_port        = g_mb_var_port + CONFIG_MB_PORT_UART4;
	p_mb_var_port->huart = &huart4;
	mb_port_init(CONFIG_MB_PORT_UART4, MB_SLAVE, MB_RTU, 1, 502, 115200, MB_PAR_NONE);
	HAL_UART_Receive_IT(p_mb_var_port->huart, &p_mb_var_port->rxdbuffer[p_mb_var_port->rxdbufpos], 1);
	for(i = 0; i < CONFIG_MB_VAR_MASTER_BUF_SIZE; i++){
		p_mb_var_port->mb_var_master_buf[i].pos = CONFIG_MASTER_POS_FREE;
	}
	p_mb_var_port->master_buf_pos = -1;
	
	p_mb_var_port        = g_mb_var_port + CONFIG_MB_PORT_UART5;
	p_mb_var_port->huart = &huart5;
	mb_port_init(CONFIG_MB_PORT_UART5, MB_SLAVE, MB_RTU, 1, 502, 115200, MB_PAR_NONE);
	HAL_UART_Receive_IT(p_mb_var_port->huart, &p_mb_var_port->rxdbuffer[p_mb_var_port->rxdbufpos], 1);	
	for(i = 0; i < CONFIG_MB_VAR_MASTER_BUF_SIZE; i++){
		p_mb_var_port->mb_var_master_buf[i].pos = CONFIG_MASTER_POS_FREE;
	}
	p_mb_var_port->master_buf_pos = -1;
	
	p_mb_var_port = g_mb_var_port + CONFIG_MB_PORT_SPI2;
	mb_port_init(CONFIG_MB_PORT_SPI2, MB_SLAVE, MB_TCP, 1, 502, 115200, MB_PAR_NONE);
	for(i = 0; i < CONFIG_MB_VAR_MASTER_BUF_SIZE; i++){
		p_mb_var_port->mb_var_master_buf[i].pos = CONFIG_MASTER_POS_FREE;
	}
	p_mb_var_port->master_buf_pos = -1;
	
	HAL_UART_Transmit(g_mb_var_port[CONFIG_MB_PORT_USART1].huart, (void *)"serial port-1\r\n", 15, 100);
//	mb_serial_rs485_send(g_mb_var_port[CONFIG_MB_PORT_USART2].huart, (void *)"serial port-2\r\n");
	HAL_UART_Transmit(g_mb_var_port[CONFIG_MB_PORT_USART3].huart, (void *)"serial port-3\r\n", 15, 100);
	HAL_UART_Transmit(g_mb_var_port[CONFIG_MB_PORT_UART4].huart, (void *)"serial port-4\r\n", 15, 100);
	HAL_UART_Transmit(g_mb_var_port[CONFIG_MB_PORT_UART5].huart, (void *)"serial port-5\r\n", 15, 100);
}

/*
*****************************************************************
* 解析
*****************************************************************
*/

//uint16_t txxx[10] = {256, 257, 258, 259, 260};
//uint8_t  tx = 0;
//uint8_t  rx = 0;
//uint8_t  rxxx[10];
uint8_t mb_user_poll(void)
{	
	if(poll_id >= CONFIG_MB_PORT_COUNT){
		poll_id = 0;
		return FALSE;
	}

	mb_port_poll( poll_id );
	poll_id++;
	
//	if(tx == 1){
//		mb_master_wrtie(CONFIG_MB_PORT_USART3, 16, 100, txxx, 5, 3000);
//		tx = 0;
//	}
//	
//	if(rx == 1){
//		mb_master_read(CONFIG_MB_PORT_USART3, 3, 100, 5, rxxx, 3000);
//		rx = 0;
//	}
	return TRUE;
}


