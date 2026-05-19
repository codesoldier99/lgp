#ifndef __MB_ASCII_H__
#define __MB_ASCII_H__

#ifdef __cplusplus
PR_BEGIN_EXTERN_C
#endif

#include "stdint.h"
#include "mb_type.h"

// 初始化
eMBErrorCode mb_ascii_init(uint8_t id, uint8_t ucSlaveAddress, uint8_t ucPort, uint32_t ulBaudRate, eMBParity eParity);

// 接收数据
void mb_ascii_receive(uint8_t id, uint8_t context);

// 发送帧数据
uint8_t mb_ascii_snd_frame(uint8_t id);

// 完善帧内容
void mb_ascii_frame_conform(uint8_t id);

// 帧判断
uint8_t mb_ascii_rev_adjust(uint8_t id);

// 帧分段（从地址 + 帧内容 + 帧长度）
uint8_t mb_ascii_segmentation( uint8_t id, uint8_t *cur_frame_address, uint8_t *cur_frame_pos, uint16_t *cur_frame_len );

#ifdef __cplusplus
PR_END_EXTERN_C
#endif
#endif
