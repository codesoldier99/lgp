#include "mb_cb.h"

#include "plc_modbus.h"

/* ----------------------- Defines ------------------------------------------*/
#define REG_INPUT_START 		30001
#define REG_INPUT_NREGS 		4
static uint16_t usRegInputBuf[REG_INPUT_NREGS];


/* ----------------------- Static variables ---------------------------------*/
#define REG_HOLDING_START 	40001
#define REG_HOLDING_END     49999

static uint16_t usRegHoldingBufSize = REG_HOLDING_SIZE;
uint8_t pusRegHoldingBuf[REG_HOLDING_SIZE];//\\modbus¼Ä´æÆ÷\\//


/*
***************************************************************************************
* ½Ó¿Ú
***************************************************************************************
*/

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

eMBErrorCode eMBRegHoldingCB( MB_VAR_PORT *p_mb_var_port, uint16_t usAddress, uint16_t usNRegs, eMBRegisterMode eMode )
{
  eMBErrorCode    eStatus = MB_ENOERR;
	int             iRegIndex;

	uint8_t *rxdbuffer = p_mb_var_port->rxdbuffer;
	uint8_t *txdbuffer = p_mb_var_port->txdbuffer;
	
	
	if( ( usAddress >= REG_HOLDING_START && usAddress <= REG_HOLDING_END) && ( usAddress + usNRegs <= REG_HOLDING_START + usRegHoldingBufSize ) )
	{
		iRegIndex = ( int )( usAddress - REG_HOLDING_START ) * 2;
		switch ( eMode ){
			case MB_REG_READ:
				while( usNRegs > 0 ){
					txdbuffer[p_mb_var_port->txdbufpos] = ( uint8_t ) ( pusRegHoldingBuf[iRegIndex + 1] );
					p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
					
					txdbuffer[p_mb_var_port->txdbufpos] = ( uint8_t ) ( pusRegHoldingBuf[iRegIndex + 0] );
					p_mb_var_port->txdbufpos = CONFIG_MB_TXD_BUFFER_POS(p_mb_var_port->txdbufpos, 1);
					
					iRegIndex += 2;
					usNRegs--;
				}
			break;

			case MB_REG_WRITE:
				while( usNRegs > 0 ){
					pusRegHoldingBuf[iRegIndex + 1] = rxdbuffer[p_mb_var_port->rxdbufcnt];
					p_mb_var_port->rxdbufcnt = CONFIG_MB_RXD_BUFFER_POS(p_mb_var_port->rxdbufcnt, 1);	
					pusRegHoldingBuf[iRegIndex + 0] = rxdbuffer[p_mb_var_port->rxdbufcnt];
					p_mb_var_port->rxdbufcnt = CONFIG_MB_RXD_BUFFER_POS(p_mb_var_port->rxdbufcnt, 1);	
					iRegIndex += 2;
					usNRegs--;
				}
			break;
		}
	}
	else
	{
		switch ( eMode )
		{
			case MB_REG_READ:
				if(plc_modbus_read(txdbuffer, &p_mb_var_port->txdbufpos, CONFIG_MB_TXD_BUFFER_MARK, usAddress, usNRegs) == TRUE)
					eStatus = MB_ENOERR;
			break;
				
			case MB_REG_WRITE:
				if(plc_modbus_write(rxdbuffer, &p_mb_var_port->rxdbufcnt, CONFIG_MB_RXD_BUFFER_MARK, usAddress, usNRegs) == TRUE)
					eStatus = MB_ENOERR;
				break;
		}
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
