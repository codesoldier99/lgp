#include "mb_rtu.h"

#include "mb_serial.h"
#include "mb_timer.h"
#include "mb_crc.h"

#include "plc_modbus.h"

#include "string.h"

/* ----------------------- Defines ------------------------------------------*/
#define MB_SER_PDU_SIZE_MIN     4       /*!< Minimum size of a Modbus RTU frame. */
#define MB_SER_PDU_SIZE_MAX     64     /*!< Maximum size of a Modbus RTU frame. */
#define MB_SER_PDU_SIZE_CRC     2       /*!< Size of CRC field in PDU. */
#define MB_SER_PDU_ADDR_OFF     0       /*!< Offset of slave address in Ser-PDU. */
#define MB_SER_PDU_PDU_OFF      1       /*!< Offset of Modbus-PDU in Ser-PDU. */

/*
*****************************************************************
* 初始化
*****************************************************************
*/
eMBErrorCode mb_rtu_init(uint8_t id, uint8_t ucSlaveAddress, uint8_t ucPort, uint32_t ulBaudRate, eMBParity eParity)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	
	eMBErrorCode    eStatus = MB_ENOERR;
	
	// 从地址
	p_mb_var_port->address = ucSlaveAddress;
	
	// 帧T35
	if( mb_serial_init( ucPort, ulBaudRate, 8, eParity ) != TRUE ){
		eStatus = MB_EPORTERR;
	}else{
		/* If baudrate > 19200(1800us) then we should use the fixed timer values t35 = 1750us. Otherwise t35 must be 3.5 times the character time.*/
		if( ulBaudRate > 19200 ){
				p_mb_var_port->frame_rcv_value = 35*1000/ulBaudRate + 1;
		}else{
				p_mb_var_port->frame_rcv_value = 2;
		}
		
		p_mb_var_port->frame_snd_value = p_mb_var_port->frame_rcv_value;
	}

  return eStatus;
}

/*
*****************************************************************
* 接收数据
*****************************************************************
*/
void mb_rtu_receive(uint8_t id, uint8_t context)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;

	p_mb_var_port->rxdbuffer[p_mb_var_port->rxdbufpos] = context;
	p_mb_var_port->rxdbufpos = CONFIG_MB_RXD_BUFFER_POS(p_mb_var_port->rxdbufpos, 1);

	mb_timer_rcv_update(p_mb_var_port);
}

/*
*****************************************************************
* 发送帧数据
*****************************************************************
*/
uint8_t mb_rtu_snd_frame(uint8_t id)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	
	//待发送判断
	if(p_mb_var_port->txdframecnt == p_mb_var_port->txdframepos)
		return FALSE;
	
	// 发送间隔
	if(mb_timer_snd_outtime(p_mb_var_port) == FALSE)
		return FALSE;
	
	//发送数据
	mb_serial_send(p_mb_var_port);
	
	return TRUE;
}


/*
*****************************************************************
* 完善帧内容
*****************************************************************
*/
void mb_rtu_frame_conform(uint8_t id)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	uint16_t usCRC16;
	
	// 从地址
	p_mb_var_port->txdbuffer[p_mb_var_port->cur_txd_frame_pos] = p_mb_var_port->address;

	// CRC16 校验
	usCRC16 = usMBCRC16( p_mb_var_port->txdbuffer, p_mb_var_port->cur_txd_frame_pos, p_mb_var_port->cur_txd_frame_len + 1, CONFIG_MB_TXD_BUFFER_MARK );
	
	// 复制校验
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->cur_txd_frame_pos + 1, p_mb_var_port->cur_txd_frame_len);
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = ( uint8_t )( usCRC16 & 0xFF );
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = ( uint8_t )( usCRC16 >> 8 );
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	
	// 插入一帧标记
	mb_serial_txd_frame_insert(p_mb_var_port);
}

/*
*****************************************************************
* 帧判断
*****************************************************************
*/
uint8_t mb_rtu_rev_adjust(uint8_t id)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	
	//新帧判断
	if(mb_timer_rcv_outtime(p_mb_var_port) == TRUE){
		mb_serial_rxd_frame_insert(p_mb_var_port);
		mb_timer_rcv_disable(p_mb_var_port);
	}
	
	//待解析帧判断
	if(p_mb_var_port->rxdframecnt != p_mb_var_port->rxdframepos){
		return TRUE;
	}else{		
		return FALSE;
	}
}

/*
*****************************************************************
* 帧分段（从地址 + 帧内容 + 帧长度）
*****************************************************************
*/
uint8_t mb_rtu_segmentation( uint8_t id, uint8_t *cur_frame_address, uint8_t *cur_frame_pos, uint16_t *cur_frame_len )
{
	MB_VAR_PORT  *p_mb_var_port = g_mb_var_port + id;	
	eMBErrorCode  eStatus = MB_ENOERR;
	uint8_t       frame_sta, frame_len;

	//获取帧信息
	mb_serial_frame_info(p_mb_var_port, &frame_sta, &frame_len);
	
	if( ( frame_len >= MB_SER_PDU_SIZE_MIN ) && ( usMBCRC16( p_mb_var_port->rxdbuffer, frame_sta, frame_len, CONFIG_MB_RXD_BUFFER_MARK) == 0 ) )
	{
		/* Save the address field. All frames are passed to the upper layed and the decision if a frame is used is done there.*/
		*cur_frame_address = p_mb_var_port->rxdbuffer[CONFIG_MB_RXD_BUFFER_POS(frame_sta, MB_SER_PDU_ADDR_OFF)];

		/* Total length of Modbus-PDU is Modbus-Serial-Line-PDU minus size of address field and CRC checksum.*/
		*cur_frame_len = ( uint16_t )( frame_len - MB_SER_PDU_PDU_OFF - MB_SER_PDU_SIZE_CRC );

		/* Return the start of the Modbus PDU to the caller. */
		*cur_frame_pos = CONFIG_MB_RXD_BUFFER_POS(frame_sta, MB_SER_PDU_PDU_OFF);
	}
	else
	{
			eStatus = MB_EIO;
	}

	return eStatus;
}

/*
***********************************************************************************
* 发内容
***********************************************************************************
*/
uint8_t mb_rtu_out(uint8_t id, uint8_t code, uint16_t address, uint16_t count)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	
	uint16_t usCRC16;
	uint8_t  pos;
	p_mb_var_port->cur_txd_frame_pos = p_mb_var_port->txdbufpos;
	p_mb_var_port->cur_txd_frame_len = count*2 + 5;
	
	// 寄存器内容
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 5);
	if(plc_modbus_read(p_mb_var_port->txdbuffer, &p_mb_var_port->txdbufpos, CONFIG_MB_TXD_BUFFER_MARK, address, count) == FALSE)
		return FALSE;
		
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
	p_mb_var_port->txdbuffer[pos] =  count*2;
	pos = CONFIG_MB_TXD_BUFFER_POS(pos, 1);
		
	// 校验
	usCRC16 = usMBCRC16( p_mb_var_port->txdbuffer, p_mb_var_port->cur_txd_frame_pos, p_mb_var_port->cur_txd_frame_len, CONFIG_MB_TXD_BUFFER_MARK );
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] =  (usCRC16>>8) & 0xFF;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] =  usCRC16 & 0xFF;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	
	// 插入一帧标记
	mb_serial_txd_frame_insert(p_mb_var_port);
	
	return TRUE;
}

uint8_t mb_rtu_out_string(uint8_t id, uint8_t code, uint16_t address, uint8_t *buf)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	
	uint16_t usCRC16;
	uint8_t  pos, i;
	
	uint16_t reg_count, buf_len = strlen((void *)buf);
	if(buf_len%2 != 0)
		reg_count = buf_len/2 + 1;
	
	p_mb_var_port->cur_txd_frame_pos = p_mb_var_port->txdbufpos;
	p_mb_var_port->cur_txd_frame_len = reg_count*2 + 5;
		
	// 寄存器内容
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 5);
	for(i = 0; i < buf_len; i++) {
		p_mb_var_port->txdbuffer[CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, i)] = buf[i];
	}if(buf_len%2 != 0){
		p_mb_var_port->txdbuffer[CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, i)] = 0;
	}
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, reg_count*2);
	
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
	p_mb_var_port->txdbuffer[pos] =  reg_count*2;
	pos = CONFIG_MB_TXD_BUFFER_POS(pos, 1);
		
	// 校验
	usCRC16 = usMBCRC16( p_mb_var_port->txdbuffer, p_mb_var_port->cur_txd_frame_pos, p_mb_var_port->cur_txd_frame_len, CONFIG_MB_TXD_BUFFER_MARK );
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] =  (usCRC16>>8) & 0xFF;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] =  usCRC16 & 0xFF;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	
	// 插入一帧标记
	mb_serial_txd_frame_insert(p_mb_var_port);
	
	return TRUE;

}

