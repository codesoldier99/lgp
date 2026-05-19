#include "mb_ascii.h"
#include "mb_crc.h"
#include "mb_serial.h"
#include "mb_timer.h"

/* ----------------------- Defines ------------------------------------------*/
#define MB_ASCII_DEFAULT_CR     '\r'    /*!< Default CR character for Modbus ASCII. */
#define MB_ASCII_DEFAULT_LF     '\n'    /*!< Default LF character for Modbus ASCII. */
#define MB_SER_PDU_SIZE_MIN     3       /*!< Minimum size of a Modbus ASCII frame. */
#define MB_SER_PDU_SIZE_MAX     64     /*!< Maximum size of a Modbus ASCII frame. */
#define MB_SER_PDU_SIZE_LRC     1       /*!< Size of LRC field in PDU. */
#define MB_SER_PDU_ADDR_OFF     0       /*!< Offset of slave address in Ser-PDU. */
#define MB_SER_PDU_PDU_OFF      1       /*!< Offset of Modbus-PDU in Ser-PDU. */

typedef enum
{
	BYTE_HIGH_NIBBLE,           /*!< Character for high nibble of byte. */
  BYTE_LOW_NIBBLE             /*!< Character for low nibble of byte. */
} eMBBytePos;

static volatile eMBBytePos eBytePos;
static volatile uint8_t    isFrameStaFlag;

/*
*****************************************************************
* 字符转换
*****************************************************************
*/
static uint8_t prvucMBCHAR2BIN( uint8_t ucCharacter )
{
	if( ( ucCharacter >= '0' ) && ( ucCharacter <= '9' ) ){
			return ( uint8_t )( ucCharacter - '0' );
	}else if( ( ucCharacter >= 'A' ) && ( ucCharacter <= 'F' ) ){
			return ( uint8_t )( ucCharacter - 'A' + 0x0A );
	}else{
			return 0xFF;
	}
}

/*
*****************************************************************
* 初始化
*****************************************************************
*/
eMBErrorCode mb_ascii_init(uint8_t id, uint8_t ucSlaveAddress, uint8_t ucPort, uint32_t ulBaudRate, eMBParity eParity)
{
	MB_VAR_PORT  *p_mb_var_port = g_mb_var_port + id;
	eMBErrorCode  eStatus = MB_ENOERR;
    
	//端口初始化
	if( mb_serial_init( ucPort, ulBaudRate, 7, eParity ) != TRUE ){
		eStatus = MB_EPORTERR;
	}else{
		p_mb_var_port->frame_rcv_value = MB_ASCII_TIMEOUT_SEC * 1000UL;
		p_mb_var_port->frame_snd_value = 2;
	}

	// 从地址
	p_mb_var_port->address = ucSlaveAddress;
	
	// 起始帧
	isFrameStaFlag = FALSE;
	
	return eStatus;
}

/*
*****************************************************************
* 接收数据
*****************************************************************
*/
void mb_ascii_receive(uint8_t id, uint8_t context)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	uint8_t ucResult;
	
	// 触发超时定时器
	mb_timer_rcv_update(p_mb_var_port);
	
	//起始符不保存
	if( context == ':' ){	
		eBytePos = BYTE_HIGH_NIBBLE;
		isFrameStaFlag = TRUE;
	}
	//结束符不保存
	else if( context == MB_ASCII_DEFAULT_LF ){	
		if(p_mb_var_port->rxdbuffer[CONFIG_MB_RXD_FRAME_POS(p_mb_var_port->rxdbufpos, -1)] == MB_ASCII_DEFAULT_CR && isFrameStaFlag == TRUE){
			p_mb_var_port->rxdframebuf[p_mb_var_port->rxdframepos] = p_mb_var_port->rxdbufpos;
			p_mb_var_port->rxdframepos =CONFIG_MB_RXD_FRAME_POS(p_mb_var_port->rxdframepos, 1);
			isFrameStaFlag = FALSE;
			
			// 触发超时定时器
			mb_timer_rcv_disable(p_mb_var_port);
		}
	}
	//内容保存
	else{
		if(isFrameStaFlag == FALSE)
			return;
		
		ucResult = prvucMBCHAR2BIN( context );
		switch ( eBytePos )
		{
			case BYTE_HIGH_NIBBLE:
				p_mb_var_port->rxdbuffer[p_mb_var_port->rxdbufpos] = ( uint8_t )( ucResult << 4 );
				eBytePos = BYTE_LOW_NIBBLE;
			break;
			
			case BYTE_LOW_NIBBLE:
				p_mb_var_port->rxdbuffer[p_mb_var_port->rxdbufpos] |= ucResult;
				p_mb_var_port->rxdbufpos = CONFIG_MB_RXD_FRAME_POS(p_mb_var_port->rxdbufpos, 1);
				eBytePos = BYTE_HIGH_NIBBLE;
			break;
		}
	}
}

/*
*****************************************************************
* 发送帧数据
*****************************************************************
*/
uint8_t mb_ascii_snd_frame(uint8_t id)
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
void mb_ascii_frame_conform(uint8_t id)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
	uint8_t           usLRC;
	
	// 从地址
	p_mb_var_port->txdbuffer[p_mb_var_port->cur_txd_frame_pos] = p_mb_var_port->address;

	// CRC16 校验和
	usLRC = prvucMBLRC( p_mb_var_port->txdbuffer, p_mb_var_port->cur_txd_frame_pos, p_mb_var_port->cur_txd_frame_len + 1, CONFIG_MB_TXD_BUFFER_MARK );
	
	// 复制校验和
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->cur_txd_frame_pos + 1, p_mb_var_port->cur_txd_frame_len);
	p_mb_var_port->txdbuffer[p_mb_var_port->txdbufpos] = usLRC;
	p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
	
	// 插入一帧标记
	mb_serial_txd_frame_insert(p_mb_var_port);
}

/*
*****************************************************************
* 帧判断
*****************************************************************
*/
uint8_t mb_ascii_rev_adjust(uint8_t id)
{
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
		
	//待解析帧判断
	if(p_mb_var_port->rxdframecnt != p_mb_var_port->rxdframepos)
		return TRUE;
	else 
		return FALSE;
}

/*
*****************************************************************
* 帧分段（从地址 + 帧内容 + 帧长度）
*****************************************************************
*/
uint8_t mb_ascii_segmentation( uint8_t id, uint8_t *cur_frame_address, uint8_t *cur_frame_pos, uint16_t *cur_frame_len )
{
  eMBErrorCode    eStatus = MB_ENOERR;
	MB_VAR_PORT *p_mb_var_port = g_mb_var_port + id;
  uint8_t       frame_sta, frame_len;

	//获取帧信息
	mb_serial_frame_info(p_mb_var_port, &frame_sta, &frame_len);	
	
	if( frame_len >= MB_SER_PDU_SIZE_MIN && prvucMBLRC( p_mb_var_port->rxdbuffer, frame_sta, frame_len, CONFIG_MB_RXD_BUFFER_MARK) == 0){
		/* Save the address field. All frames are passed to the upper layed and the decision if a frame is used is done there.*/
		*cur_frame_address = p_mb_var_port->rxdbuffer[CONFIG_MB_RXD_BUFFER_POS(frame_sta, MB_SER_PDU_ADDR_OFF)];
		/* Total length of Modbus-PDU is Modbus-Serial-Line-PDU minus size of address field and CRC checksum.*/
		*cur_frame_len = ( uint16_t )( frame_len - MB_SER_PDU_PDU_OFF - MB_SER_PDU_SIZE_LRC );
		/* Return the start of the Modbus PDU to the caller. */
		*cur_frame_pos = CONFIG_MB_RXD_BUFFER_POS(frame_sta, MB_SER_PDU_PDU_OFF);
	}
	else
	{
		eStatus = MB_EIO;
	}

	return eStatus;
}
