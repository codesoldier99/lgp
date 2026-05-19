#ifndef __ETH_W5500_H__
#define __ETH_W5500_H__

#include "eth_type.h"

/* Operation mode bits */
#define W5500_RWB_VDM		0x00
#define W5500_RWB_FDM1		0x01
#define	W5500_RWB_FDM2		0x02
#define W5500_RWB_FDM4		0x03

/* Read_Write control bit */
#define W5500_RWB_READ	0x00
#define W5500_RWB_WRITE	0x04

/* Block select bits */
#define W5500_COMMON_R	0x00

#define W5500_REG_MR		0x0000
	#define W5500_REG_MR__RST		0x80
	#define W5500_REG_MR__WOL		0x20
	#define W5500_REG_MR__PB		0x10
	#define W5500_REG_MR__PPP		0x08
	#define W5500_REG_MR__FARP	0x02
	
#define W5500_REG_GAR		0x0001
#define W5500_REG_SUBR	0x0005
#define W5500_REG_SHAR	0x0009
#define W5500_REG_SIPR	0x000f

#define W5500_REG_INTLEVEL	0x0013

#define W5500_REG_IR		0x0015
	#define W5500_REG_IR__CONFLICT	0x80
	#define W5500_REG_IR__UNREACH		0x40
	#define W5500_REG_IR__PPPoE			0x20
	#define W5500_REG_IR__MP				0x10

#define W5500_REG_IMR		0x0016
	#define W5500_REG_IMR__IM_IR7		0x80
	#define W5500_REG_IMR__IM_IR6		0x40
	#define W5500_REG_IMR__IM_IR5		0x20
	#define W5500_REG_IMR__IM_IR4		0x10

#define W5500_REG_SIR		0x0017
	#define W5500_REG_SIR__S7_INT		0x80
	#define W5500_REG_SIR__S6_INT		0x40
	#define W5500_REG_SIR__S5_INT		0x20
	#define W5500_REG_SIR__S4_INT		0x10
	#define W5500_REG_SIR__S3_INT		0x08
	#define W5500_REG_SIR__S2_INT		0x04
	#define W5500_REG_SIR__S1_INT		0x02
	#define W5500_REG_SIR__S0_INT		0x01

#define W5500_REG_SIMR	0x0018
	#define W5500_REG_SIMR__S7_IMR		0x80
	#define W5500_REG_SIMR__S6_IMR		0x40
	#define W5500_REG_SIMR__S5_IMR		0x20
	#define W5500_REG_SIMR__S4_IMR		0x10
	#define W5500_REG_SIMR__S3_IMR		0x08
	#define W5500_REG_SIMR__S2_IMR		0x04
	#define W5500_REG_SIMR__S1_IMR		0x02
	#define W5500_REG_SIMR__S0_IMR		0x01

#define W5500_REG_RTR			0x0019
#define W5500_REG_RCR			0x001b

#define W5500_REG_PTIMER	0x001c
#define W5500_REG_PMAGIC	0x001d
#define W5500_REG_PHA			0x001e
#define W5500_REG_PSID		0x0024
#define W5500_REG_PMRU		0x0026

#define W5500_REG_UIPR		0x0028
#define W5500_REG_UPORT		0x002c

#define W5500_REG_PHYCFGR	0x002e
	#define W5500_REG_PHYCFGR__RST_PHY		0x80
	#define W5500_REG_PHYCFGR__OPMODE			0x40
	#define W5500_REG_PHYCFGR__DPX				0x04
	#define W5500_REG_PHYCFGR__SPD				0x02
	#define W5500_REG_PHYCFGR__LINK				0x01

#define W5500_REG_VERR	0x0039

uint8_t ETH_W5500_ReadByte1(uint16_t reg);

void ETH_W5500_WriteByte1(uint16_t reg, uint8_t dat);

void ETH_W5500_LocalVarConfig(void);

void ETH_W5500_BaseRegConfig(void);

void ETH_W5500_LocalRegConfig(void);
#endif

