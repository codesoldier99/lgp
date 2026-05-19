#ifndef __ETH_SOCKET_H__
#define __ETH_SOCKET_H__

#include <stdint.h>

typedef uint8_t SOCKET_TypeDef;			//自定义端口号数据类型

/********************* Socket Register *******************/
#define SOCKETn_MR		0x0000
	#define MULTI_MFEN		0x80
	#define BCASTB				0x40
	#define	ND_MC_MMB			0x20
	#define UCASTB_MIP6B	0x10
	#define MR_CLOSE			0x00
	#define MR_TCP				0x01
	#define MR_UDP				0x02
	#define MR_MACRAW			0x04

#define SOCKETn_CR		0x0001
typedef enum{
	SOCKET_OPEN      = 0x01,
	SOCKET_LISTEN    = 0x02,
	SOCKET_CONNECT   = 0x04,
	SOCKET_DISCON    = 0x08,
	SOCKET_CLOSE     = 0x10,
	SOCKET_SEND      = 0x20,
	SOCKET_SEND_MAC  = 0x21,
	SOCKET_SEND_KEEP = 0x22,
	SOCKET_RECV      = 0x40
}SOCKET_CR_ENUM;

#define SOCKETn_IR		0x0002
typedef enum{
	SOCKET_IR_SEND = 0x10,
	SOCKET_IR_TIMEOUT = 0x08,
	SOCKET_IR_RECV    = 0x04,
	SOCKET_IR_DISCON  = 0x02,
	SOCKET_IR_CON     = 0x01
}SOCKET_IR_ENUM;

#define SOCKETn_SR		0x0003
	#define SOCK_CLOSED				0x00
	#define SOCK_INIT					0x13
	#define SOCK_LISTEN				0x14
	#define SOCK_ESTABLISHED	0x17
	#define SOCK_CLOSE_WAIT		0x1c
	#define SOCK_UDP					0x22
	#define SOCK_MACRAW				0x02

	#define SOCK_SYNSEND			0x15
	#define SOCK_SYNRECV			0x16
	#define SOCK_FIN_WAI			0x18
	#define SOCK_CLOSING			0x1a
	#define SOCK_TIME_WAIT		0x1b
	#define SOCK_LAST_ACK			0x1d

#define SOCKETn_PORT				0x0004
#define SOCKETn_DHAR	  		0x0006
#define SOCKETn_DIPR				0x000c
#define SOCKETn_DPORTR			0x0010

#define SOCKETn_MSSR				0x0012
#define SOCKETn_TOS					0x0015
#define SOCKETn_TTL					0x0016

#define SOCKETn_RXBUF_SIZE	0x001e
#define SOCKETn_TXBUF_SIZE	0x001f
#define SOCKETn_TX_FSR			0x0020
#define SOCKETn_TX_RD				0x0022
#define SOCKETn_TX_WR				0x0024
#define SOCKETn_RX_RSR			0x0026
#define SOCKETn_RX_RD				0x0028
#define SOCKETn_RX_WR				0x002a

#define SOCKETn_IMR					0x002c
	#define IMR_SENDOK	0x10
	#define IMR_TIMEOUT	0x08
	#define IMR_RECV		0x04
	#define IMR_DISCON	0x02
	#define IMR_CON			0x01

#define SOCKETn_FRAG				0x002d
#define SOCKETn_KPALVTR			0x002f

///*******************************************************************/
///************************ SPI Control Byte *************************/
///*******************************************************************/

/* Socket 0 */
#define S0_REG		0x08
#define S0_TX_BUF	0x10
#define S0_RX_BUF	0x18

/* Socket 1 */
#define S1_REG		0x28
#define S1_TX_BUF	0x30
#define S1_RX_BUF	0x38

/* Socket 2 */
#define S2_REG		0x48
#define S2_TX_BUF	0x50
#define S2_RX_BUF	0x58

/* Socket 3 */
#define S3_REG		0x68
#define S3_TX_BUF	0x70
#define S3_RX_BUF	0x78

/* Socket 4 */
#define S4_REG		0x88
#define S4_TX_BUF	0x90
#define S4_RX_BUF	0x98

/* Socket 5 */
#define S5_REG		0xa8
#define S5_TX_BUF	0xb0
#define S5_RX_BUF	0xb8

/* Socket 6 */
#define S6_REG		0xc8
#define S6_TX_BUF	0xd0
#define S6_RX_BUF	0xd8

/* Socket 7 */
#define S7_REG		0xe8
#define S7_TX_BUF	0xf0
#define S7_RX_BUF	0xf8

#define S_RX_SIZE	2048	/*定义Socket接收缓冲区的大小，可以根据W5500_RMSR的设置修改 */
#define S_TX_SIZE	2048  /*定义Socket发送缓冲区的大小，可以根据W5500_TMSR的设置修改 */

///***************----- 端口数据缓冲区 -----***************/

uint8_t ETH_SOCKET_ReadByte1(SOCKET_TypeDef s, uint16_t reg);

uint16_t ETH_SOCKET_WriteBuffer(SOCKET_TypeDef s, uint8_t *dat_ptr, uint16_t size);

uint16_t ETH_SOCKET_ReadBuffer(SOCKET_TypeDef s, uint8_t *dat_ptr, uint8_t size);
void ETH_SOCKET_ReadByteN(SOCKET_TypeDef s, uint8_t *dat_ptr, uint16_t len);

void ETH_SOCKET_RemoteVarConfig(SOCKET_TypeDef s_id, uint16_t l_port, uint8_t *d_ip, uint16_t d_port);

void ETH_SOCKET_RemoteRegConfig(SOCKET_TypeDef s_id);

void ETH_W5500_IrStatus(void);

uint8_t ETH_SOCKET_IrStatus(uint8_t s_id);
uint8_t ETH_SOCKET_SrStatus(uint8_t s_id);

uint8_t ETH_SOCKET_Connect(SOCKET_TypeDef s);
uint8_t ETH_SOCKET_TcpListen(SOCKET_TypeDef s);

void ETH_SOCKET_Close(SOCKET_TypeDef s);

#endif
