#ifndef __MB_TYPE_H__
#define __MB_TYPE_H__

#include <assert.h>
#include <inttypes.h>

#include "stm32f1xx_hal.h"

#ifndef TRUE
	#define TRUE		1
#endif

#ifndef FALSE
	#define FALSE 	0
#endif

/* ----------------------- Type definitions ---------------------------------*/
/*! \ingroup modbus
 * \brief Parity used for characters in serial mode.
 *
 * The parity which should be applied to the characters sent over the serial
 * link. Please note that this values are actually passed to the porting
 * layer and therefore not all parity modes might be available.
 */
typedef enum
{
	MB_PAR_NONE,                /*!< No parity. */
	MB_PAR_ODD,                 /*!< Odd parity. */
  MB_PAR_EVEN                 /*!< Even parity. */
} eMBParity;

/* ----------------------- Type definitions ---------------------------------*/

typedef enum
{
	BYTE_HIGH_NIBBLE,           /*!< Character for high nibble of byte. */
  BYTE_LOW_NIBBLE             /*!< Character for low nibble of byte. */
} eMBBytePos;

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
  EV_READY,                   /*!< Startup finished. */
  EV_FRAME_RECEIVED,          /*!< Frame received. */
  EV_EXECUTE,                 /*!< Execute function. */
  EV_FRAME_SENT,               /*!< Frame sent. */
	EV_FRAME_ERROR,
} eMBEventType;

typedef enum
{
    STATE_RX_INIT,              /*!< Receiver is in initial state. */
    STATE_RX_IDLE,              /*!< Receiver is in idle state. */
    STATE_RX_RCV,               /*!< Frame is beeing received. */
    STATE_RX_ERROR,             /*!< If the frame is invalid. */
		STATE_RX_WAIT_EOF
} eMBRcvState;

typedef enum
{
    STATE_TX_IDLE,              /*!< Transmitter is in idle state. */
    STATE_TX_XMIT,               /*!< Transmitter is in transfer state. */
    STATE_TX_START,             /*!< Starting transmission (':' sent). */
    STATE_TX_DATA,              /*!< Sending of data (Address, Data, LRC). */
    STATE_TX_END,               /*!< End of transmission. */
    STATE_TX_NOTIFY             /*!< Notify sender that the frame has been sent. */
} eMBSndState;

typedef struct{
	eMBEventType eQueuedEvent;
	uint8_t     xEventInQueue;
	
	eMBRcvState eRcvState;
	eMBSndState eSndState;
	
	uint8_t  ucRcvBuffer[64];
	uint16_t usRcvBufPos;
	uint16_t usRcvBufCnt;
	
	uint8_t ucTxdBuffer[64];
	uint8_t ucTxdBufPos;
	uint8_t ucTxdBufCnt;
	
	uint8_t *pucSndBufferCur;
	uint16_t usSndBufferCount;
	
	UART_HandleTypeDef *huart;
	
	uint32_t timer_value;
	uint32_t timer_tick;
}MB_VAR_TypeDef;

typedef struct{
	eMBErrorCode ( *peMBFrameReceive ) ( MB_VAR_TypeDef *pMbVarStruct, uint8_t * pucRcvAddress, uint8_t ** pucFrame, uint16_t * pusLength );
	void    		 ( *pvMBFrameStart )   ( MB_VAR_TypeDef *pMbVarStruct );
  void    		 ( *pvMBFrameStop )    ( MB_VAR_TypeDef *pMbVarStruct );
	eMBErrorCode ( *peMBFrameSend )    ( MB_VAR_TypeDef *pMbVarStruct, uint8_t slaveAddress, const uint8_t * pucFrame, uint16_t usLength );
  void         ( *pvMBFrameClose )   ( MB_VAR_TypeDef *pMbVarStruct );
	
	uint8_t				 ( *pxMBFrameCBByteReceived )     ( MB_VAR_TypeDef *pMbVarStruct );
	uint8_t				 ( *pxMBFrameCBTransmitterEmpty ) ( MB_VAR_TypeDef *pMbVarStruct );
	uint8_t				 ( *pxMBPortCBTimerExpired )      ( MB_VAR_TypeDef *pMbVarStruct );
}MB_FUNC_TypeDef;

typedef struct{
	MB_VAR_TypeDef  var_struct;
	MB_FUNC_TypeDef func_struct;
}MB_PORT_TypeDef;

#endif

