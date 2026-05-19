#ifndef __MB_CB_H__
#define __MB_CB_H__

#include "mb_type.h"

#define REG_HOLDING_SIZE		48
extern uint8_t pusRegHoldingBuf[REG_HOLDING_SIZE];

eMBErrorCode eMBRegInputCB( uint8_t * pucRegBuffer, uint16_t usAddress, uint16_t usNRegs );

eMBErrorCode eMBRegHoldingCB( MB_VAR_PORT *p_mb_var_port, uint16_t usAddress, uint16_t usNRegs, eMBRegisterMode eMode );

eMBErrorCode eMBRegCoilsCB( uint8_t * pucRegBuffer, uint16_t usAddress, uint16_t usNCoils, eMBRegisterMode eMode );

eMBErrorCode eMBRegDiscreteCB( uint8_t * pucRegBuffer, uint16_t usAddress, uint16_t usNDiscrete );

#endif
