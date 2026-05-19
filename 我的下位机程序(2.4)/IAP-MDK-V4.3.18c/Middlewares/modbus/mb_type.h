#ifndef __MB_TYPE_H__
#define __MB_TYPE_H__

#include "stm32f1xx_hal.h"
#include "mb_config.h"

#ifndef TRUE
	#define TRUE	1
#endif

#ifndef FALSE
	#define FALSE	0
#endif

/*! \ingroup modbus
 * \brief Errorcodes used by all function in the protocol stack.
 */
typedef enum
{
    MB_ENOERR,                  /*!< no error. */
    MB_ENOREG,                  /*!< illegal register address. */
    MB_EINVAL,                  /*!< illegal argument. */
    MB_EPORTERR,                /*!< porting layer error. */
    MB_ENORES,                  /*!< insufficient resources. */
    MB_EIO,                     /*!< I/O error. */
    MB_EILLSTATE,               /*!< protocol stack in illegal state. */
    MB_ETIMEDOUT                /*!< timeout error occurred. */
} eMBErrorCode;

typedef enum
{
	MB_SLAVE,
	MB_MASTER,
} eMBCommMode;

typedef enum
{
    MB_RTU,    /*!< RTU transmission mode. */
    MB_ASCII,  /*!< ASCII transmission mode. */
    MB_TCP     /*!< TCP mode. */
} eMBMode;

typedef enum
{
	MB_PAR_NONE, /*!< No parity. */
	MB_PAR_ODD,  /*!< Odd parity. */
  MB_PAR_EVEN  /*!< Even parity. */
} eMBParity;


typedef enum
{
  MB_EV_READY,            /*!< Startup finished. */
  MB_EV_FRAME_SEGMENTATION,   /*!< Frame received. */
  MB_EV_EXECUTE_SLAVE,          /*!< Execute function. */
	MB_EV_EXECUTE_MASTER,
  MB_EV_FRAME_SENT,       /*!< Frame sent. */
	MB_EV_FRAME_ERROR,
} eMBEventType;

typedef enum
{
	MB_REG_READ,                /*!< Read register values and pass to protocol stack. */
	MB_REG_WRITE                /*!< Update register values. */
} eMBRegisterMode;

/*
******************************************************************************/

typedef enum{
	CONFIG_RS485_RECEIVE = 0,
	CONFIG_RS485_SEND,
}MB_RS485_PORT;

typedef enum{
	CONFIG_MB_PORT_USART1 = 0,
	CONFIG_MB_PORT_USART2,
	CONFIG_MB_PORT_USART3,
	CONFIG_MB_PORT_UART4,
	CONFIG_MB_PORT_UART5,
	CONFIG_MB_PORT_SPI2,
	CONFIG_MB_PORT_COUNT,
}MB_PORT_ID;

#define CONFIG_MB_RXD_BUFFER_SIZE	64
#define CONFIG_MB_RXD_BUFFER_MARK	(CONFIG_MB_RXD_BUFFER_SIZE - 1)
#define CONFIG_MB_RXD_FRAME_SIZE 	4
#define CONFIG_MB_RXD_FRAME_MARK	(CONFIG_MB_RXD_FRAME_SIZE - 1)

#define CONFIG_MB_TXD_BUFFER_SIZE 64
#define CONFIG_MB_TXD_BUFFER_MARK	(CONFIG_MB_TXD_BUFFER_SIZE - 1)
#define CONFIG_MB_TXD_FRAME_SIZE	4
#define CONFIG_MB_TXD_FRAME_MARK	(CONFIG_MB_TXD_FRAME_SIZE - 1)

#define CONFIG_MB_RXD_BUFFER_POS(POS, OFFSET)	((POS + OFFSET) & CONFIG_MB_RXD_BUFFER_MARK)
#define CONFIG_MB_RXD_FRAME_POS(POS, OFFSET)	((POS + OFFSET) & CONFIG_MB_RXD_FRAME_MARK)

#define CONFIG_MB_TXD_BUFFER_POS(POS, OFFSET)	((POS + OFFSET) & CONFIG_MB_TXD_BUFFER_MARK)
#define CONFIG_MB_TXD_FRAME_POS(POS, OFFSET)	((POS + OFFSET) & CONFIG_MB_TXD_FRAME_MARK)

typedef enum{
	CONFIG_MASTER_POS_FREE    = -2,
	CONFIG_MASTER_POS_RESULT  = -1,
	CONFIG_MASTER_POS_RUNNING = 0,
}MB_MASTER_POS_STATUS;

typedef struct{
	 int8_t   pos;
	uint8_t  *buf;
}MB_VAR_MASTER_BUF;
#define CONFIG_MB_VAR_MASTER_BUF_SIZE	8

typedef struct{
	eMBCommMode comm_mode;
	uint8_t     mode;
	
	uint8_t 		address;
	
	 int8_t     master_buf_pos;
	uint32_t    out_time_value;
	uint32_t    out_time_mark;
	
	uint8_t     master_wr_flag;
	
	UART_HandleTypeDef *huart;
	
	uint8_t rxdbuffer[CONFIG_MB_RXD_BUFFER_SIZE];
	uint8_t rxdbufpos;
	uint8_t rxdbufcnt;
	uint8_t rxdframebuf[CONFIG_MB_RXD_FRAME_SIZE];
	uint8_t rxdframepos;
	uint8_t rxdframecnt;
	
	uint8_t txdbuffer[CONFIG_MB_TXD_BUFFER_SIZE];
	uint8_t txdbufpos;
	uint8_t txdbufcnt;
	uint8_t txdframebuf[CONFIG_MB_TXD_FRAME_SIZE];
	uint8_t txdframepos;
	uint8_t txdframecnt;
	
	//帧
	uint32_t frame_rcv_tick;
	uint32_t frame_rcv_value;
	
	//发送
	uint32_t frame_snd_tick;
	uint32_t frame_snd_value;
	
	//处理变量
	void    (*mb_receive)(uint8_t id, uint8_t context);
	uint8_t (*mb_rev_adjust)(uint8_t id);
	uint8_t (*mb_snd_frame)(uint8_t id);
	uint8_t (*mb_segmentation)( uint8_t id, uint8_t *pucRcvAddress, uint8_t *cur_frame_pos, uint16_t *pusLength );
	void    (*mb_frame_conform)(uint8_t id);
	
	//过程变量
	uint8_t  poll_event;
	uint8_t  cur_frame_address;
	uint8_t  ucFunctionCode;
	uint8_t  cur_rxd_frame_pos;
	uint16_t cur_rxd_frame_len;
	
	uint8_t  cur_txd_frame_pos;
	uint16_t cur_txd_frame_len;
	
	//从机响应
	MB_VAR_MASTER_BUF mb_var_master_buf[CONFIG_MB_VAR_MASTER_BUF_SIZE];
}MB_VAR_PORT;

extern MB_VAR_PORT g_mb_var_port[CONFIG_MB_PORT_COUNT];

#endif
