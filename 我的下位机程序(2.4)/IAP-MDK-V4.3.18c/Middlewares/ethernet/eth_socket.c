
#include "eth_spi.h"
#include "eth_w5500.h"
#include "eth_socket.h"
#include "eth_type.h"

/*
******************************************************************************
* 函数名  :
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  : 
* 说明    : 
******************************************************************************
*/
void ETH_SOCKET_WriteByte1(SOCKET_TypeDef s, uint16_t reg, uint8_t dat)
{
	//置W5500的SCS为低电平	
	ETH_SPI_SetCS(LOW);
		
	//通过SPI写16位寄存器地址
	EHT_SPI_WriteByte2(reg);
	
	//通过SPI写控制字节,1个字节数据长度,写数据,选择端口s的寄存器
	EHT_SPI_WriteByte1(W5500_RWB_FDM1 | W5500_RWB_WRITE | (s*0x20 + 0x08));
	
	//写1个字节数据
	EHT_SPI_WriteByte1(dat);

	//置W5500的SCS为高电平
	ETH_SPI_SetCS(HIGH); 
}

void ETH_SOCKET_WriteByte2(SOCKET_TypeDef s, uint16_t reg, uint16_t dat)
{
	//置W5500的SCS为低电平
	ETH_SPI_SetCS(LOW);
			
	//通过SPI写16位寄存器地址
	EHT_SPI_WriteByte2(reg);
	
	//通过SPI写控制字节,2个字节数据长度,写数据,选择端口s的寄存器
	EHT_SPI_WriteByte1(W5500_RWB_FDM2 | W5500_RWB_WRITE | (s*0x20 + 0x08));
	
	//写16位数据
	EHT_SPI_WriteByte2(dat);

	//置W5500的SCS为高电平
	ETH_SPI_SetCS(HIGH); 
}

void ETH_SOCKET_WriteByte4(SOCKET_TypeDef s, uint16_t reg, uint8_t *dat_ptr)
{
	//置W5500的SCS为低电平
	ETH_SPI_SetCS(LOW);
			
	//通过SPI写16位寄存器地址
	EHT_SPI_WriteByte2(reg);
	
	//通过SPI写控制字节,4个字节数据长度,写数据,选择端口s的寄存器
	EHT_SPI_WriteByte1(W5500_RWB_FDM4 | W5500_RWB_WRITE | (s*0x20+0x08));

	//写入数据
	EHT_SPI_WriteByte1(*dat_ptr++);//写第1个字节数据
	EHT_SPI_WriteByte1(*dat_ptr++);//写第2个字节数据
	EHT_SPI_WriteByte1(*dat_ptr++);//写第3个字节数据
	EHT_SPI_WriteByte1(*dat_ptr++);//写第4个字节数据

	//置W5500的SCS为高电平
	ETH_SPI_SetCS(HIGH); 
}

/*
******************************************************************************
* 函数名  :
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  : 
* 说明    : 
******************************************************************************
*/
uint8_t ETH_SOCKET_ReadByte1(SOCKET_TypeDef s, uint16_t reg)
{
	uint8_t value;

	//置W5500的SCS为低电平
	ETH_SPI_SetCS(LOW);
			
	//通过SPI写16位寄存器地址
	EHT_SPI_WriteByte2(reg);
	
	//通过SPI写控制字节,1个字节数据长度,读数据,选择端口s的寄存器
	EHT_SPI_WriteByte1(W5500_RWB_FDM1 | W5500_RWB_READ | (s*0x20 + 0x08));

	//读取数据
	value = EHT_SPI_ReadByte1();//读取1个字节数据

	//置W5500的SCS为高电平
	ETH_SPI_SetCS(HIGH);
	
	return value;//返回读取到的寄存器数据
}

uint16_t ETH_SOCKET_ReadByte2(SOCKET_TypeDef s, uint16_t reg)
{
	uint16_t value;

	//置W5500的SCS为低电平
	ETH_SPI_SetCS(LOW);
			
	//通过SPI写16位寄存器地址
	EHT_SPI_WriteByte2(reg);
	
	//通过SPI写控制字节,2个字节数据长度,读数据,选择端口s的寄存器
	EHT_SPI_WriteByte1(W5500_RWB_FDM2 | W5500_RWB_READ | (s*0x20 + 0x08));

	//读取数据
	value = EHT_SPI_ReadByte1()<<8;//读取高位数据
	value |= EHT_SPI_ReadByte1();//读取低位数据

	//置W5500的SCS为高电平
	ETH_SPI_SetCS(HIGH);
	
	//返回读取到的寄存器数据
	return value;
}

/*
******************************************************************************
* 函数名  :
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  : 
* 说明    : 
******************************************************************************
*/
uint16_t ETH_SOCKET_ReadBuffer(SOCKET_TypeDef s, uint8_t *dat_ptr, uint8_t size)
{
	uint16_t rx_size, w_rx_addr, i;
	
	//获取数据尺寸
	rx_size = ETH_SOCKET_ReadByte2(s, SOCKETn_RX_RSR);
	if(rx_size == 0) 		//没接收到数据则返回
		return 0;
	if(rx_size > 1460) 
		rx_size = 1460;
		
	if(rx_size >= size)	//最多尺寸
		rx_size = size;
	
	//获取w5500发送缓存起始地址
	w_rx_addr  = ETH_SOCKET_ReadByte2(s, SOCKETn_RX_RD) & (S_RX_SIZE - 1);

	//置W5500的SCS为低电平
	ETH_SPI_SetCS(LOW);

	//写16位地址
	EHT_SPI_WriteByte2(w_rx_addr);
	
	//写控制字节,N个字节数据长度,读数据,选择端口s的寄存器
	EHT_SPI_WriteByte1(W5500_RWB_VDM | W5500_RWB_READ | (s*0x20 + 0x18));
	
	//读取数据
	if((w_rx_addr + rx_size) < S_RX_SIZE)	{//如果最大地址未超过W5500接收缓冲区寄存器的最大地址
		for(i = 0; i < rx_size; i++){
			*dat_ptr = EHT_SPI_ReadByte1();
			dat_ptr++;
		}
	}else	{//如果最大地址超过W5500接收缓冲区寄存器的最大地址
	
		//前半段
		w_rx_addr = S_RX_SIZE - w_rx_addr;
		for(i = 0; i < w_rx_addr; i++){
			*dat_ptr = EHT_SPI_ReadByte1();
			dat_ptr++;
		}
		ETH_SPI_SetCS(HIGH);

		//后半段
		ETH_SPI_SetCS(LOW);

		EHT_SPI_WriteByte2(0x00);
		EHT_SPI_WriteByte1(W5500_RWB_VDM | W5500_RWB_READ | (s*0x20 + 0x18));

		for(; i < rx_size; i++){
			*dat_ptr = EHT_SPI_ReadByte1();
			dat_ptr++;
		}
	}
	ETH_SPI_SetCS(HIGH);

	//更新实际物理地址,即下次读取接收到的数据的起始地址
	w_rx_addr = (w_rx_addr + rx_size) & (S_RX_SIZE - 1);
	ETH_SOCKET_WriteByte2(s, SOCKETn_RX_RD, w_rx_addr);
	
	//发送启动接收命令
	ETH_SOCKET_WriteByte1(s, SOCKETn_CR, SOCKET_RECV);
	
	return rx_size;//返回接收到数据的长度
}

void ETH_SOCKET_ReadByteN(SOCKET_TypeDef s, uint8_t *dat_ptr, uint16_t len)
{
	uint16_t rx_size, w_rx_addr, i;
	uint16_t rx_len = len;
	//获取数据尺寸
	rx_size = ETH_SOCKET_ReadByte2(s, SOCKETn_RX_RSR);
	if(rx_size == 0) //没接收到数据则返回
		return;
	if(rx_size > 1460) 
		rx_size = 1460;

	//获取w5500发送缓存起始地址
	w_rx_addr = ETH_SOCKET_ReadByte2(s, SOCKETn_RX_RD) & (S_RX_SIZE - 1);

	//置W5500的SCS为低电平
	ETH_SPI_SetCS(LOW);

	//写16位地址
	EHT_SPI_WriteByte2(w_rx_addr);
	
	//写控制字节,N个字节数据长度,读数据,选择端口s的寄存器
	EHT_SPI_WriteByte1(W5500_RWB_VDM | W5500_RWB_READ | (s*0x20 + 0x18));
	
	//读取数据
	if((w_rx_addr + rx_size) < S_RX_SIZE)	{//如果最大地址未超过W5500接收缓冲区寄存器的最大地址
		for(i = 0; i < rx_size && len != 0; i++, len--){
			*dat_ptr = EHT_SPI_ReadByte1();
			dat_ptr++;
		}
		
		if(len <= 0)
			goto exit;
	}else	{//如果最大地址超过W5500接收缓冲区寄存器的最大地址
		//前半段
		w_rx_addr = S_RX_SIZE - w_rx_addr;
		for(i = 0; i < w_rx_addr && len != 0; i++, len--){
			*dat_ptr = EHT_SPI_ReadByte1();
			dat_ptr++;
		}
		if(len <= 0)
			goto exit;
		ETH_SPI_SetCS(HIGH);

		//后半段
		ETH_SPI_SetCS(LOW);

		EHT_SPI_WriteByte2(0x00);
		EHT_SPI_WriteByte1(W5500_RWB_VDM | W5500_RWB_READ | (s*0x20 + 0x18));

		for(; i < rx_size && len != 0; i++, len--){
			*dat_ptr = EHT_SPI_ReadByte1();
			dat_ptr++;
		}
		if(len <= 0)
			goto exit;
	}
	
exit:
	ETH_SPI_SetCS(HIGH);

	//更新实际物理地址,即下次读取接收到的数据的起始地址
	w_rx_addr = (w_rx_addr + rx_len) & (S_RX_SIZE - 1);
	ETH_SOCKET_WriteByte2(s, SOCKETn_RX_RD, w_rx_addr);
	
	//发送启动接收命令
	ETH_SOCKET_WriteByte1(s, SOCKETn_CR, SOCKET_RECV);
}

uint16_t ETH_SOCKET_WriteBuffer(SOCKET_TypeDef s, uint8_t *dat_ptr, uint16_t size)
{
	uint16_t i, w_tx_addr, w_tx_mark;

	//如果是UDP模式,可以在此设置目的主机的IP和端口号
	if((ETH_SOCKET_ReadByte1(s, SOCKETn_MR) & 0x0f) != SOCK_UDP){//如果Socket打开失败	
		ETH_SOCKET_WriteByte4(s, SOCKETn_DIPR,   ETH_SOCKET_Struct.remoteConfig[0].udp_dst_ip);//设置目的主机IP  		
		ETH_SOCKET_WriteByte2(s, SOCKETn_DPORTR, ETH_SOCKET_Struct.remoteConfig[0].udp_dst_port[0]*256 + ETH_SOCKET_Struct.remoteConfig[0].udp_dst_port[1]);//设置目的主机端口号				
	}

	//获取w5500发送缓存起始地址
	w_tx_mark = w_tx_addr = ETH_SOCKET_ReadByte2(s, SOCKETn_TX_WR);
	w_tx_addr &= (S_TX_SIZE - 1);//计算实际的物理地址

	//置W5500的SCS为低电平
	ETH_SPI_SetCS(LOW);
	
	//写16位地址
	EHT_SPI_WriteByte2(w_tx_addr);
	
	//写控制字节,N个字节数据长度,写数据,选择端口s的寄存器
	EHT_SPI_WriteByte1(W5500_RWB_VDM | W5500_RWB_WRITE | (s*0x20 + 0x10));

	//写入数据
	if((w_tx_addr + size) < S_TX_SIZE)	{//如果最大地址未超过W5500发送缓冲区寄存器的最大地址
		for(i = 0; i < size; i++)//循环写入size个字节数据
			EHT_SPI_WriteByte1(*dat_ptr++);//写入一个字节的数据		
	}else	{//如果最大地址超过W5500发送缓冲区寄存器的最大地址
		//前半段
		uint16_t len = S_TX_SIZE - w_tx_addr;
		for(i = 0; i < len; i++)//循环写入前w_rx_addr个字节数据
			EHT_SPI_WriteByte1(*dat_ptr++);//写入一个字节的数据
		
		ETH_SPI_SetCS(HIGH); //置W5500的SCS为高电平

		//后半段
		ETH_SPI_SetCS(LOW);//置W5500的SCS为低电平

		EHT_SPI_WriteByte2(0x00);//写16位地址
		EHT_SPI_WriteByte1(W5500_RWB_VDM | W5500_RWB_WRITE | (s*0x20 + 0x10));//写控制字节,N个字节数据长度,写数据,选择端口s的寄存器

		for(; i < size; i++)	{//循环写入size-w_rx_addr个字节数据
			EHT_SPI_WriteByte1(*dat_ptr++);//写入一个字节的数据
		}
	}
	ETH_SPI_SetCS(HIGH); //置W5500的SCS为高电平

	//更新实际物理地址,即下次写待发送数据到发送数据缓冲区的起始地址
	w_tx_mark += size;
	ETH_SOCKET_WriteByte2(s, SOCKETn_TX_WR, w_tx_mark);
	
	//发送启动发送命令
	ETH_SOCKET_WriteByte1(s, SOCKETn_CR, SOCKET_SEND);
	
	return size;
}

/*
******************************************************************************
* 函数名  : 
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  :
* 说明    :
******************************************************************************
*/
void ETH_SOCKET_RemoteVarConfig(SOCKET_TypeDef s_id, uint16_t l_port/*5000*/,\
	uint8_t *d_ip,/*192.168.1.100*/ uint16_t d_port/*6000*/)
{
	ETH_REMOTE_CONFIG_TypeDef *p_remote_config = &ETH_SOCKET_Struct.remoteConfig[s_id];
	
	//加载端口0的端口号5000 
	p_remote_config->local_port[0] = l_port/256; p_remote_config->local_port[1] = l_port%256;
	
	//加载端口0的目的IP地址
	p_remote_config->tcp_dst_ip[0] = d_ip[0]; p_remote_config->tcp_dst_ip[1] = d_ip[1]; 
	p_remote_config->tcp_dst_ip[2] = d_ip[2];   p_remote_config->tcp_dst_ip[3] = d_ip[3];
	
	//加载端口0的目的端口号6000
	p_remote_config->tcp_dst_port[0] = d_port/256; p_remote_config->tcp_dst_port[1] = d_port%256;
	
	//加载端口0的工作模式,TCP客户端模式
	p_remote_config->mode   = socket_mode_tcp_client;
}

/*
******************************************************************************
* 函数名  : 
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  :
* 说明    :
******************************************************************************
*/
void ETH_SOCKET_RemoteRegConfig(SOCKET_TypeDef s_id)
{
	ETH_REMOTE_CONFIG_TypeDef *p_remote_config = &ETH_SOCKET_Struct.remoteConfig[s_id];
	
	//设置分片长度，参考W5500数据手册，该值可以不修改(最大分片字节数=1460(0x5b4))
	ETH_SOCKET_WriteByte2(s_id, SOCKETn_MSSR, 1460);
	
	//其它是Socket事件中断
	ETH_SOCKET_WriteByte1(s_id, SOCKETn_IMR, IMR_SENDOK | IMR_TIMEOUT | IMR_RECV | IMR_DISCON | IMR_CON);
	
	//设置发送缓冲区和接收缓冲区的大小，参考W5500数据手册
	do{
		ETH_SOCKET_WriteByte1(s_id, SOCKETn_RXBUF_SIZE, 0x02);//Socket Rx memory size=2k
	}while(ETH_SOCKET_ReadByte1(s_id, SOCKETn_RXBUF_SIZE) != 0x02);
				
	do{
		ETH_SOCKET_WriteByte1(s_id, SOCKETn_TXBUF_SIZE, 0x02);//Socket Tx mempry size=2k
	}while(ETH_SOCKET_ReadByte1(s_id, SOCKETn_TXBUF_SIZE) != 0x02);
	
	do{
		ETH_SOCKET_WriteByte2(s_id, SOCKETn_TX_WR, 0);
	}while(ETH_SOCKET_ReadByte2(s_id, SOCKETn_TX_WR) != 0);
	
	do{
		ETH_SOCKET_WriteByte2(s_id, SOCKETn_RX_RD, 0);
	}while(ETH_SOCKET_ReadByte2(s_id, SOCKETn_RX_RD) != 0);
		
	//设置指定端口
	ETH_SOCKET_WriteByte2(s_id, SOCKETn_PORT,   p_remote_config->local_port[0]*256 + p_remote_config->local_port[1]);
	ETH_SOCKET_WriteByte2(s_id, SOCKETn_DPORTR, p_remote_config->tcp_dst_port[0]*256 + p_remote_config->tcp_dst_port[1]);
	ETH_SOCKET_WriteByte4(s_id, SOCKETn_DIPR,   p_remote_config->tcp_dst_ip);	
}

/*******************************************************************************
* 函数名  : ETH_SOCKET_Close
* 描述    : 设置指定Socket(0~7)为客户端与远程服务器连接
* 输入    : s:待设定的端口
* 输出    : 无
* 返回值  : 成功返回TRUE(0xFF),失败返回FALSE(0x00)
* 说明    : 当本机Socket工作在客户端模式时,引用该程序,与远程服务器建立连接
*			如果启动连接后出现超时中断，则与服务器连接失败,需要重新调用该程序连接
*			该程序每调用一次,就与服务器产生一次连接
*******************************************************************************/
void ETH_SOCKET_Close(SOCKET_TypeDef s)
{
	ETH_SOCKET_WriteByte1(s, SOCKETn_CR, SOCKET_CLOSE);	//关闭端口
}

/*
******************************************************************************
* 函数名  : 
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  :
* 说明    :
******************************************************************************
*/
void ETH_W5500_IrStatus(void)
{
	uint8_t err;
	
	err = ETH_W5500_ReadByte1(W5500_REG_IR);
	if(err != 0)
	{
		ETH_W5500_WriteByte1(W5500_REG_IR, (err & 0xf0));
		if(err & W5500_REG_IR__CONFLICT)//IP冲突
			;
		if(err & W5500_REG_IR__UNREACH)	//目标不可抵达
			;
		if(err & W5500_REG_IR__PPPoE)		//PPPoE连接关闭
			;
		if(err & W5500_REG_IR__MP)			//Magic Packet
			;
	}
}

uint8_t ETH_SOCKET_IrStatus(uint8_t s_id)
{
	uint8_t state;
	
	state = ETH_SOCKET_ReadByte1(s_id, SOCKETn_IR);
	ETH_SOCKET_WriteByte1(s_id, SOCKETn_IR, state);
	
	return state;
}


uint8_t ETH_SOCKET_SrStatus(uint8_t s_id)
{
	return ETH_SOCKET_ReadByte1(s_id, SOCKETn_SR);
}
/*******************************************************************************
* 函数名  : socket_udp
* 描述    : 设置指定Socket(0~7)为UDP模式
* 输入    : s:待设定的端口
* 输出    : 无
* 返回值  : 成功返回TRUE(0xFF),失败返回FALSE(0x00)
* 说明    : 如果Socket工作在UDP模式,引用该程序,在UDP模式下,Socket通信不需要建立连接
*			该程序只调用一次，就使W5500设置为UDP模式
*******************************************************************************/
uint8_t socket_udp(SOCKET_TypeDef s)
{
	uint32_t delayT;
	
	ETH_SOCKET_WriteByte1(s, SOCKETn_MR, MR_UDP);			//设置Socket为UDP模式*/
	ETH_SOCKET_WriteByte1(s, SOCKETn_CR, SOCKET_OPEN);	//打开Socket*/
	
	for(delayT = 0; delayT < 65535; delayT++);
	
	if(ETH_SOCKET_ReadByte1(s, SOCKETn_SR) != SOCK_UDP){//如果Socket打开失败
		ETH_SOCKET_WriteByte1(s,SOCKETn_CR, SOCKET_CLOSE);
		return FALSE;
	}else{
		return TRUE;
	}
	
	//至此完成了Socket的打开和UDP模式设置,在这种模式下它不需要与远程主机建立连接
	//因为Socket不需要建立连接,所以在发送数据前都可以设置目的主机IP和目的Socket的端口号
	//如果目的主机IP和目的Socket的端口号是固定的,在运行过程中没有改变,那么也可以在这里设置
}

/*******************************************************************************
* 函数名  : ETH_SOCKET_Connect
* 描述    : 设置指定Socket(0~7)为客户端与远程服务器连接
* 输入    : s:待设定的端口
* 输出    : 无
* 返回值  : 成功返回TRUE(0xFF),失败返回FALSE(0x00)
* 说明    : 当本机Socket工作在客户端模式时,引用该程序,与远程服务器建立连接
*			如果启动连接后出现超时中断，则与服务器连接失败,需要重新调用该程序连接
*			该程序每调用一次,就与服务器产生一次连接
*******************************************************************************/
uint8_t ETH_SOCKET_Connect(SOCKET_TypeDef s)
{	
	uint8_t state;
	
	ETH_SOCKET_WriteByte1(s, SOCKETn_MR, MR_TCP);			//设置socket为TCP模式
	ETH_SOCKET_WriteByte1(s, SOCKETn_CR, SOCKET_OPEN);	//打开Socket
	
	state = ETH_SOCKET_ReadByte1(s, SOCKETn_SR);
	if(state != SOCK_INIT)	{//如果socket打开失败
		return FALSE;
	}
	
	ETH_SOCKET_WriteByte1(s, SOCKETn_CR, SOCKET_CONNECT);//设置Socket为Connect模式
	return TRUE;
}

/*******************************************************************************
* 函数名  : ETH_SOCKET_TcpListen
* 描述    : 设置指定Socket(0~7)作为服务器等待远程主机的连接
* 输入    : s:待设定的端口
* 输出    : 无
* 返回值  : 成功返回TRUE(0xFF),失败返回FALSE(0x00)
* 说明    : 当本机Socket工作在服务器模式时,引用该程序,等等远程主机的连接
*			该程序只调用一次,就使W5500设置为服务器模式
*******************************************************************************/
uint8_t ETH_SOCKET_TcpListen(SOCKET_TypeDef s)
{
	//关闭
	ETH_SOCKET_WriteByte1(s, SOCKETn_CR, SOCKET_CLOSE);
	
	//TCP模式 & 打开Socket
	ETH_SOCKET_WriteByte1( s, SOCKETn_MR, MR_TCP );
	ETH_SOCKET_WriteByte1( s, SOCKETn_CR, SOCKET_OPEN );
	if(ETH_SOCKET_ReadByte1( s, SOCKETn_SR ) != SOCK_INIT){
		return FALSE;
	}	
	
	//设置侦听模式
	ETH_SOCKET_WriteByte1(s, SOCKETn_CR, SOCKET_LISTEN);
	if(ETH_SOCKET_ReadByte1(s, SOCKETn_SR) != SOCK_LISTEN){
		return FALSE;
	}

	return TRUE;

	//至此完成了Socket的打开和设置侦听工作,至于远程客户端是否与它建立连接,则需要等待Socket中断，
	//以判断Socket的连接是否成功。参考W5500数据手册的Socket中断状态
	//在服务器侦听模式不需要设置目的IP和目的端口号
}

