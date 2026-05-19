
#include <stdint.h>
#include "eth_type.h"

/* ±‰¡ø */
ETH_SOCKET_TypeDef ETH_SOCKET_Struct;

/* Local VAR */
const uint8_t  ETH_CONFIG_LOCAL_GATEWAY[4] = {192, 168,   0, 1};
const uint8_t  ETH_CONFIG_LOCAL_SUBMASK[4] = {255, 255, 255, 0};
const uint8_t  ETH_CONFIG_LOCAL_PHYADDR[6] = {0x0c, 0x29, 0xab, 0x7c, 0x00, 0x01};
const uint8_t  ETH_CONFIG_LOCAL_IP[4]	     = {192, 168, 0, 199};

/* Modbus TCP */
const uint8_t  ETH_CONFIG_MODBUS_SOCKET      = 0;
const uint8_t  ETH_CONFIG_MODBUS_IP[4]       = {192, 168, 0, 123};
const uint16_t ETH_CONFIG_MODBUS_REMOTE_PORT = 8080;
const uint16_t ETH_CONFIG_MODBUS_LOCAL_PORT  = 502;
