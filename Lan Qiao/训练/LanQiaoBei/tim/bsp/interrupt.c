#include "interrupt.h"

struct keys key[4]={0,0,0};
void HAL_TIM_PeriodElapsedCallback(TIM_HandleTypeDef *htim)
{
	if(htim->Instance==TIM4)
	{
		key[0].key_sta=HAL_GPIO_ReadPin(GPIOB,GPIO_PIN_0);
		key[1].key_sta=HAL_GPIO_ReadPin(GPIOB,GPIO_PIN_1);
		key[2].key_sta=HAL_GPIO_ReadPin(GPIOB,GPIO_PIN_2);
		key[3].key_sta=HAL_GPIO_ReadPin(GPIOA,GPIO_PIN_0);
		
		for(int i=0;i<4;i++)
		{
			switch (key[i].judge_sta)
			{
				case 0:
				{
					if(key[i].key_sta==0) 
					{
						key[i].judge_sta=1;
						key[i].key_time=0;
					}
				}
				break;
				case 1:
				{
					if(key[i].key_sta==0)
					{
						key[i].judge_sta=2;
					}
					else key[i].judge_sta=0;
				}
				break;
				case 2:
				{
					if(key[i].key_sta==1)
					{
						key[i].judge_sta=0;					
						if(key[i].key_time<70)
						{
							key[i].single_flag=1;
						}
					}
					else 
					{
						key[i].key_time++;
						if(key[i].key_time>70)  key[i].long_flag=1;
					}
				}
				break;
				
			}
		
		}
	}
}

////////////////////////////////////仅测量频率///////////////////
//uint ccr1_val1=0,ccr1_val2=0;
//uint frq1=0,frq2=0;
//void HAL_TIM_IC_CaptureCallback(TIM_HandleTypeDef *htim)
//{
//	if(htim->Instance==TIM2)
//	{
//		ccr1_val1=HAL_TIM_ReadCapturedValue(htim,TIM_CHANNEL_1);
//		__HAL_TIM_SetCounter(htim,0);
//		frq1=(80000000/80)/ccr1_val1;
//		HAL_TIM_IC_Start(htim,TIM_CHANNEL_1);
//	}
//	if(htim->Instance==TIM3)
//	{
//		ccr1_val2=HAL_TIM_ReadCapturedValue(htim,TIM_CHANNEL_1);
//		__HAL_TIM_SetCounter(htim,0);
//		frq2=(80000000/80)/ccr1_val2;
//		HAL_TIM_IC_Start(htim,TIM_CHANNEL_1);
//	}
//}


/////////////////////////////测量频率+占空比///////////////////
double ccr1_val1a=0,ccr1_val2a=0;
uint ccr1_val1b=0,ccr1_val2b=0;
uint frq1=0,frq2=0;//频率
float duty1=0,duty2=0;//占空比
void HAL_TIM_IC_CaptureCallback(TIM_HandleTypeDef *htim)
{
	if(htim->Instance==TIM2)
	{
		if(htim->Channel==HAL_TIM_ACTIVE_CHANNEL_1)//中断消息来源 选择直接输入的通道
		{
			ccr1_val1a=HAL_TIM_ReadCapturedValue(htim,TIM_CHANNEL_1);//直接
			ccr1_val1b=HAL_TIM_ReadCapturedValue(htim,TIM_CHANNEL_2);//间接
			__HAL_TIM_SetCounter(htim,0);
			frq1=(80000000/80)/ccr1_val1a;
			duty1=(ccr1_val1b/ccr1_val1a)*100;
			HAL_TIM_IC_Start(htim,TIM_CHANNEL_1);
			HAL_TIM_IC_Start(htim,TIM_CHANNEL_2);
		}
	}
	if(htim->Instance==TIM3)
	{
		if(htim->Channel==HAL_TIM_ACTIVE_CHANNEL_1)//中断消息来源 选择直接输入的通道
		{
			ccr1_val2a=HAL_TIM_ReadCapturedValue(htim,TIM_CHANNEL_1);
			ccr1_val2b=HAL_TIM_ReadCapturedValue(htim,TIM_CHANNEL_2);
			__HAL_TIM_SetCounter(htim,0);
			frq2=(80000000/80)/ccr1_val2a;
			duty2=(ccr1_val2b/ccr1_val2a)*100;
			HAL_TIM_IC_Start(htim,TIM_CHANNEL_1);
			HAL_TIM_IC_Start(htim,TIM_CHANNEL_2);
		}
	}
}
