#ifndef __ETH_SPI_H__
#define __ETH_SPI_H__

#include "stdint.h"

#define HIGH	1
#define LOW		0

void ETH_SPI_SetCS(uint8_t val);
	
void ETH_SPI_SetRst(uint8_t val);
	
void EHT_SPI_WriteByte1(uint8_t dat);
void EHT_SPI_WriteByte2(uint16_t dat);

uint8_t EHT_SPI_ReadByte1(void);

uint32_t EHT_GET_TICK(void);
#endif
