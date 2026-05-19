
#include "mb_crc.h"
#include "mb_serial.h"

#include "string.h"

#include "mb_master.h"

/*
****************************************************************
* 写寄存器
****************************************************************
*/
int8_t mb_master_get_buf_address(MB_VAR_PORT *p_mb_var_port)
{
	uint8_t i = 0;
	
	for(i = 0; i < CONFIG_MB_VAR_MASTER_BUF_SIZE; i++){
		if(p_mb_var_port->mb_var_master_buf[i].pos == (int8_t)CONFIG_MASTER_POS_FREE)
			return i;
	}
	
	return -1;
}

int8_t mb_master_get_buf_id(MB_VAR_PORT *p_mb_var_port, uint8_t txd_sta_pos)
{
	int8_t i = 0;
	
	for(i = 0; i < CONFIG_MB_VAR_MASTER_BUF_SIZE; i++){
		if(p_mb_var_port->mb_var_master_buf[i].pos == txd_sta_pos)
			return i;
	}
	
	return -1;
}


/* 待释放缓存缓存地址  */


/*
****************************************************************
* 写寄存器
****************************************************************
*/
uint8_t mb_master_wrtie(uint8_t id, uint8_t code, uint16_t address, uint16_t *buf, uint8_t len, uint32_t out_time)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	
	uint16_t usCRC16;
	uint8_t  pos, i, master_buf_pos;
	
	// 位置
	p_mb_var_port->cur_txd_frame_pos = p_mb_var_port->txdbufpos;
	p_mb_var_port->cur_txd_frame_len = len*2 + 7;
		
	// 寄存器内容
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 7);
	for(i = 0; i < len; i++) {
		p_mb_var_port->txdbuffer[CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, i*2 + 0)] = buf[i]/256;
		p_mb_var_port->txdbuffer[CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, i*2 + 1)] = buf[i]%256;
	}
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, len*2);
	
	// 主机响应缓存地址
	p_mb_var_port->out_time_value = out_time;
	master_buf_pos = mb_master_get_buf_address(p_mb_var_port);
	p_mb_var_port->mb_var_master_buf[master_buf_pos].pos = p_mb_var_port->cur_txd_frame_pos;
	
	// 从机地址
	p_mb_var_port->txdbuffer[p_mb_var_port->cur_txd_frame_pos] = p_mb_var_port->address;
	pos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->cur_txd_frame_pos, 1);
	
	// 功能码
	p_mb_var_port->txdbuffer[pos] = code;
	pos = CONFIG_MB_TXD_BUFFER_POS(pos, 1);

	// 寄存器地址
	p_mb_var_port->txdbuffer[pos] = (address>>8) & 0xFF;
	pos = CONFIG_MB_TXD_BUFFER_POS(pos, 1);
	p_mb_var_port->txdbuffer[pos] =  address & 0xFF;
	pos = CONFIG_MB_TXD_BUFFER_POS(pos, 1);
	
	// 寄存器数量
	p_mb_var_port->txdbuffer[pos] = len/256;
	pos = CONFIG_MB_TXD_BUFFER_POS(pos, 1);
	p_mb_var_port->txdbuffer[pos] = len%256;
	pos = CONFIG_MB_TXD_BUFFER_POS(pos, 1);
	
	// 寄存器数量
	p_mb_var_port->txdbuffer[pos] = len*2;
	pos = CONFIG_MB_TXD_BUFFER_POS(pos, 1);
		
	// 校验
	usCRC16 = usMBCRC16( p_mb_var_port->txdbuffer, p_mb_var_port->cur_txd_frame_pos, p_mb_var_port->cur_txd_frame_len, CONFIG_MB_TXD_BUFFER_MARK );
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = usCRC16 & 0xFF;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = (usCRC16>>8) & 0xFF;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	
	// 插入一帧标记
	mb_serial_txd_frame_insert(p_mb_var_port);

	return master_buf_pos;

}
/*
****************************************************************
* 读寄存器
****************************************************************
*/
MB_VAR_MASTER_BUF *mb_master_read(uint8_t id, uint8_t code, uint16_t address, uint8_t len, uint8_t *buf, uint32_t out_time)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	uint16_t usCRC16;
	uint8_t  master_buf_pos;
	
	p_mb_var_port->cur_txd_frame_pos = p_mb_var_port->txdbufpos;
	p_mb_var_port->cur_txd_frame_len = 6;
	
	// 主机响应缓存地址
	p_mb_var_port->out_time_value = out_time;
	master_buf_pos = mb_master_get_buf_address(p_mb_var_port);
	p_mb_var_port->mb_var_master_buf[master_buf_pos].pos = p_mb_var_port->txdbufpos;
	
	// 从机地址
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = p_mb_var_port->address;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	
	// 功能码
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = code;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);

	// 寄存器地址
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = (address>>8) & 0xFF;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] =  address & 0xFF;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	
	// 寄存器数量
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = len/256;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = len%256;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	
	// 缓存地址
	p_mb_var_port->mb_var_master_buf[master_buf_pos].buf = buf;
	
	// 校验
	usCRC16 = usMBCRC16( p_mb_var_port->txdbuffer, p_mb_var_port->cur_txd_frame_pos, p_mb_var_port->cur_txd_frame_len, CONFIG_MB_TXD_BUFFER_MARK );
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = usCRC16 & 0xFF;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = (usCRC16>>8) & 0xFF;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	
	// 插入一帧标记
	mb_serial_txd_frame_insert(p_mb_var_port);
	
	return &p_mb_var_port->mb_var_master_buf[master_buf_pos];
}

