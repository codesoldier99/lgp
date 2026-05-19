#ifndef __PLC_MODBUS_H__
#define __PLC_MODBUS_H__

#include "plc_type.h"

uint8_t plc_modbus_read(uint8_t *txdbuf, uint8_t *pos, uint8_t mask, uint16_t address, uint16_t count);
uint8_t plc_modbus_write(uint8_t *rxdbuf, uint8_t *pos, uint8_t mask, uint16_t address, uint16_t count);

#endif
