#ifndef __ETH_CONFIG_H__
#define __ETH_CONFIG_H__

#include <stdint.h>

#ifndef TRUE
	#define TRUE	1
#endif

#ifndef FALSE
	#define FALSE	0
#endif

typedef enum
{ 
  socket_mode_tcp_server = 0x00,	//TCP服务器模式
  socket_mode_tcp_client = 0x01,	//TCP客户端模式 
  socket_mode_udp        = 0x02	//UDP(广播)模式 
}socket_mode;

typedef enum
{ 
	socket_start_ready      = 0x00,
	socket_start_init_ok	  = 0x01,	//端口完成初始化 
	socket_start_connect_ok = 0x02	//端口完成连接,可以正常传输数据 
}socket_start;

typedef enum
{
	socket_state_ready 	  = 0x00,			
	socket_state_rx_flag  = 0x01,	//端口接收到一个数据包 
	socket_state_tx_finsh = 0x02	//端口发送一个数据包完成 
}socket_state;
	
typedef struct
{
	//网络参数变量定义
	uint8_t gateway[4];//网关IP地址 
	uint8_t submask[4];	//子网掩码 
	uint8_t phyaddr[6];	//物理地址(MAC) 
	
	uint8_t localip[4];	//本机IP地址 
}ETH_LOCAL_CONFIG_TypeDef;

typedef struct
{
	uint8_t  *tx_buff;
	uint16_t  tx_cnt;
	uint16_t  tx_pro;
	uint16_t  tx_mask;	
}socketTxd;

typedef struct
{
	uint8_t  *rx_buff;
	uint16_t  rx_cnt;	
	uint16_t  rx_pro;
	uint16_t  rx_mask;	
	
	uint8_t   rx_event;
}socketRxd;

typedef struct
{
	//网络参数变量定义
	uint8_t local_port[2];		//本机端口号(5000)
	
	uint8_t tcp_dst_ip[4];		//TCP目的地IP地址
	uint8_t tcp_dst_port[2];	//TCP目的地端口号(6000)
	
	uint8_t udp_dst_ip[4];		//UDP(广播)模式,目的主机IP地址
	uint8_t udp_dst_port[2];	//UDP(广播)模式,目的主机端口号
	
	//端口的运行模式(0 = TCP服务器模式；1 = TCP客户端模式， 2 = UDP(广播)模式)
	uint8_t mode;	
}ETH_REMOTE_CONFIG_TypeDef;

typedef struct
{
	uint8_t  enable;
	uint8_t  irq_flag;
  uint8_t	 irq_id;
	
	ETH_LOCAL_CONFIG_TypeDef  localConfig;
	
	ETH_REMOTE_CONFIG_TypeDef remoteConfig[8];
}ETH_SOCKET_TypeDef;

extern ETH_SOCKET_TypeDef ETH_SOCKET_Struct;

/* mqtt publish*/
typedef struct
{
	int8_t topic[16];
	int8_t message[32];
} ETH_MQTT_PUBLISH_TypeDef;

/* Local Var */
extern const uint8_t  ETH_CONFIG_LOCAL_GATEWAY[4];
extern const uint8_t  ETH_CONFIG_LOCAL_SUBMASK[4];
extern const uint8_t  ETH_CONFIG_LOCAL_PHYADDR[6];
extern const uint8_t  ETH_CONFIG_LOCAL_IP[4];
	
/* Modbus TCP */
extern const uint8_t  ETH_CONFIG_MODBUS_SOCKET;
extern const uint8_t  ETH_CONFIG_MODBUS_IP[4];
extern const uint16_t ETH_CONFIG_MODBUS_REMOTE_PORT;
extern const uint16_t ETH_CONFIG_MODBUS_LOCAL_PORT;

/* MQTT TCP */
extern const uint8_t  ETH_CONFIG_MQTT_SOCKET;
extern const uint8_t  ETH_CONFIG_MQTT_IP[4];
extern const uint16_t ETH_CONFIG_MQTT_REMOTE_PORT;
extern const uint16_t ETH_CONFIG_MQTT_LOCAL_PORT;

#endif
