#ifndef __MB_MASTER_H__
#define __MB_MASTER_H__

#include "mb_type.h"

// »ñÈ¡Ö÷»ú·¢ËÍ»º´æµØÖ·
int8_t mb_master_get_buf_id(MB_VAR_PORT *p_mb_var_port, uint8_t txd_sta_pos);

// Ğ´¼Ä´æÆ÷
uint8_t mb_master_wrtie(uint8_t id, uint8_t code, uint16_t address, uint16_t *buf, uint8_t len, uint32_t out_time);

// ¶Á¼Ä´æÆ÷
MB_VAR_MASTER_BUF *mb_master_read(uint8_t id, uint8_t code, uint16_t address, uint8_t len, uint8_t *buf, uint32_t out_time);

#endif
