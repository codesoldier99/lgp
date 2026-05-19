#include "eth_spi.h"
#include "eth_type.h"
#include "eth_w5500.h"

/*
******************************************************************************
* 函数名  : 
* 描述    : 数组比较
* 输入    : 
* 输出    : 
* 返回值  : 
* 说明    :
******************************************************************************
*/
static uint8_t ETH_W5500_ArrayCompare(uint8_t *src, uint8_t *dst, uint8_t size)
{
	uint8_t i = 0;
	
	for(i = 0; i < size; i++){
		if(src[i] != dst[i])
			return FALSE;
	}
	return TRUE;
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
void ETH_W5500_WriteByte1(uint16_t reg, uint8_t dat)
{
	//置W5500的SCS为低电平
	ETH_SPI_SetCS(LOW);

	//通过SPI写16位寄存器地址
	EHT_SPI_WriteByte2(reg);
	
	//通过SPI写控制字节,1个字节数据长度,写数据,选择通用寄存器
	EHT_SPI_WriteByte1(W5500_RWB_FDM1 | W5500_RWB_WRITE | W5500_COMMON_R);
	
	//写1个字节数据
	EHT_SPI_WriteByte1(dat);

	//置W5500的SCS为高电平
	ETH_SPI_SetCS(HIGH); 
}

void ETH_W5500_WriteByte2(uint16_t reg, uint16_t dat)
{
	//置W5500的SCS为低电平
	ETH_SPI_SetCS(LOW);
		
	//通过SPI写16位寄存器地址
	EHT_SPI_WriteByte2(reg);
	
	//通过SPI写控制字节,2个字节数据长度,写数据,选择通用寄存器
	EHT_SPI_WriteByte1(W5500_RWB_FDM2 | W5500_RWB_WRITE | W5500_COMMON_R);
	
	//写16位数据
	EHT_SPI_WriteByte2(dat);

	//置W5500的SCS为高电平
	ETH_SPI_SetCS(HIGH); 
}

void ETH_W5500_WriteBuffer(uint16_t reg, uint8_t *dat_ptr, uint16_t size)
{
	uint16_t i;

	//置W5500的SCS为低电平	
	ETH_SPI_SetCS(LOW);
		
	//通过SPI写16位寄存器地址
	EHT_SPI_WriteByte2(reg);
	
	//通过SPI写控制字节,N个字节数据长度,写数据,选择通用寄存器
	EHT_SPI_WriteByte1(W5500_RWB_VDM | W5500_RWB_WRITE | W5500_COMMON_R);

	//循环将缓冲区的size个字节数据写入W5500
	for(i=0; i<size; i++)
		EHT_SPI_WriteByte1(*dat_ptr++);//写一个字节数据

	//置W5500的SCS为高电平
	ETH_SPI_SetCS(HIGH); 
}

/*
******************************************************************
* 函数名  : 
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  : 
* 说明    : 
******************************************************************
*/
uint8_t ETH_W5500_ReadByte1(uint16_t reg)
{
	uint8_t value;

	//SCS为低电平
	ETH_SPI_SetCS(LOW);
			
	//写入数据
	EHT_SPI_WriteByte2(reg);//通过SPI写16位寄存器地址
	EHT_SPI_WriteByte1(W5500_RWB_FDM1 | W5500_RWB_READ | W5500_COMMON_R);//通过SPI写控制字节,1个字节数据长度,读数据,选择通用寄存器

	//读出数据
	value = EHT_SPI_ReadByte1();

	//SCS为高电平
	ETH_SPI_SetCS(HIGH);
	
	//返回读取到的寄存器数据
	return value;
}

void ETH_W5500_ReadBuffer(uint16_t reg, uint8_t *dat_ptr, uint16_t size)
{
	uint16_t idx = 0;
	
	//SCS为低电平
	ETH_SPI_SetCS(LOW);
			
	//写入数据
	EHT_SPI_WriteByte2(reg);//通过SPI写16位寄存器地址
	EHT_SPI_WriteByte1(W5500_RWB_VDM | W5500_RWB_READ | W5500_COMMON_R);//通过SPI写控制字节,1个字节数据长度,读数据,选择通用寄存器

	//读出数据
	for(idx = 0; idx < size; idx++) {                  // Write data in loop
    dat_ptr[idx] = EHT_SPI_ReadByte1();
  }

	//SCS为高电平
	ETH_SPI_SetCS(HIGH);
}
/*
******************************************************************
* 函数名  : 
* 描述    : 
* 输入    : 
* 输出    : 
* 返回值  : 
* 说明    : 
******************************************************************
*/
void ETH_W5500_WriteCompare(uint16_t reg, uint8_t *dat_ptr, uint16_t size)
{
	uint8_t i;
	uint8_t value;
	
	for(i = 0; i < size; i++)
	{
		do{
			ETH_W5500_WriteByte1(reg + i, dat_ptr[i]);
			value = ETH_W5500_ReadByte1(reg + i);
		}while(value != dat_ptr[i]);
	}
}

void ETH_W5500_WriteCheck(uint16_t reg, uint8_t *dat_ptr, uint16_t size)
{
	uint8_t rxd[12] = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12};
	
	do{
		ETH_W5500_WriteBuffer(reg, dat_ptr, size);
		ETH_W5500_ReadBuffer(reg, rxd, size);
	}while(ETH_W5500_ArrayCompare(dat_ptr, rxd, size) == FALSE);
}	

/*******************************************************************************
* 函数名  : ETH_W5500_LocalVarConfig
* 描述    : 装载网络参数
* 输入    : 无
* 输出    : 无
* 返回值  : 无
* 说明    : 网关、掩码、物理地址、本机IP地址、端口号、目的IP地址、目的端口号、端口工作模式
*******************************************************************************/
void ETH_W5500_LocalVarConfig(void)
{
	ETH_LOCAL_CONFIG_TypeDef *p_local_config_struct = &ETH_SOCKET_Struct.localConfig;
	
	//加载网关参数
	p_local_config_struct->gateway[0] = ETH_CONFIG_LOCAL_GATEWAY[0]; 
	p_local_config_struct->gateway[1] = ETH_CONFIG_LOCAL_GATEWAY[1]; 
	p_local_config_struct->gateway[2] = ETH_CONFIG_LOCAL_GATEWAY[2]; 
	p_local_config_struct->gateway[3] = ETH_CONFIG_LOCAL_GATEWAY[3];
	//加载子网掩码
	p_local_config_struct->submask[0] = ETH_CONFIG_LOCAL_SUBMASK[0]; 
	p_local_config_struct->submask[1] = ETH_CONFIG_LOCAL_SUBMASK[1]; 
	p_local_config_struct->submask[2] = ETH_CONFIG_LOCAL_SUBMASK[2];	
	p_local_config_struct->submask[3] = ETH_CONFIG_LOCAL_SUBMASK[3];
	//加载物理地址
	p_local_config_struct->phyaddr[0] = ETH_CONFIG_LOCAL_PHYADDR[0]; 
	p_local_config_struct->phyaddr[1] = ETH_CONFIG_LOCAL_PHYADDR[1]; 
	p_local_config_struct->phyaddr[2] = ETH_CONFIG_LOCAL_PHYADDR[2]; 
	p_local_config_struct->phyaddr[3] = ETH_CONFIG_LOCAL_PHYADDR[3]; 
	p_local_config_struct->phyaddr[4] = ETH_CONFIG_LOCAL_PHYADDR[4]; 
	p_local_config_struct->phyaddr[5] = ETH_CONFIG_LOCAL_PHYADDR[5]; 
	//加载本机IP地址
	p_local_config_struct->localip[0] = ETH_CONFIG_LOCAL_IP[0];
	p_local_config_struct->localip[1] = ETH_CONFIG_LOCAL_IP[1];
	p_local_config_struct->localip[2] = ETH_CONFIG_LOCAL_IP[2];
	p_local_config_struct->localip[3] = ETH_CONFIG_LOCAL_IP[3];
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
void ETH_W5500_BaseRegConfig(void)
{
	uint8_t outtime_time[2] = {0x07, 0xd0};	
	uint8_t outtime_cnt[1] = {0x08};
	
	//软件复位W5500,置1有效,复位后自动清0
	ETH_W5500_WriteByte1(W5500_REG_MR, W5500_REG_MR__RST);
	while((ETH_W5500_ReadByte1(W5500_REG_MR) & W5500_REG_MR__RST) != 0);
	
	//设置重试时间，默认为2000(200ms) 
	//每一单位数值为100微秒,初始化时值设为2000(0x07D0),等于200毫秒
	ETH_W5500_WriteCheck(W5500_REG_RTR, outtime_time, 2);

	//设置重试次数，默认为8次 
	//如果重发的次数超过设定值,则产生超时中断(相关的端口中断寄存器中的Sn_IR 超时位(TIMEOUT)置“1”)
	ETH_W5500_WriteCheck(W5500_REG_RTR, outtime_cnt, 1);

	//启动中断，参考W5500数据手册确定自己需要的中断类型
	//IMR_CONFLICT是IP地址冲突异常中断,IMR_UNREACH是UDP通信时，地址无法到达的异常中断
	ETH_W5500_WriteByte1(W5500_REG_IMR, W5500_REG_IMR__IM_IR7 | W5500_REG_IMR__IM_IR6);
	ETH_W5500_WriteByte1(W5500_REG_SIMR, W5500_REG_SIMR__S0_IMR);
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
void ETH_W5500_LocalRegConfig(void)
{
	ETH_LOCAL_CONFIG_TypeDef *p_local_config_struct = &ETH_SOCKET_Struct.localConfig;
	
	//设置物理地址,PHY_ADDR为6字节uint8_t数组,自己定义,用于唯一标识网络设备的物理地址值
	//该地址值需要到IEEE申请，按照OUI的规定，前3个字节为厂商代码，后三个字节为产品序号
	//如果自己定义物理地址，注意第一个字节必须为偶数
	ETH_W5500_WriteCheck(W5500_REG_SHAR, p_local_config_struct->phyaddr, 6);
	
	//设置子网掩码(MASK)值,SUB_MASK为4字节uint8_t数组,自己定义
	//子网掩码用于子网运算
	ETH_W5500_WriteCheck(W5500_REG_SUBR, p_local_config_struct->submask, 4);
	
	//设置网关(Gateway)的IP地址,gateway为4字节uint8_t数组,自己定义 
	//使用网关可以使通信突破子网的局限，通过网关可以访问到其它子网或进入Internet
	ETH_W5500_WriteCheck(W5500_REG_GAR, p_local_config_struct->gateway, 4);
	
	//设置本机的IP地址,IP_ADDR为4字节uint8_t数组,自己定义
	//注意，网关IP必须与本机IP属于同一个子网，否则本机将无法找到网关
	ETH_W5500_WriteCheck(W5500_REG_SIPR, p_local_config_struct->localip, 4);
}
