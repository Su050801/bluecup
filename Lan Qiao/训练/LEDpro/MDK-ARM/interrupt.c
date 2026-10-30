#include "interrupt.h"
#include "usart.h "
struct keys key[4]={0,0,0,0};

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
						if(key[i].key_time<200)
						{
							key[i].single_flag=1;
						}
					}
					else 
					{
						key[i].key_time++;
						if(key[i].key_time>200)
						key[i].long_flag=1;
					}
				}
				break;
				
			}
		
		}
	}
}
double ccrl_val1a=0,ccrl_val2a=0;
uint ccrl_val1b=0,ccrl_val2b=0;
uint frq1=0,frq2=0;
float duty1=0, duty2=0;
void HAL_TIM_IC_CaptureCallback(TIM_HandleTypeDef *htim)
{
	
	if(htim->Instance==TIM2)
	{
		if(htim->Channel==HAL_TIM_ACTIVE_CHANNEL_1)
		{
			ccrl_val1a=HAL_TIM_ReadCapturedValue(htim,TIM_CHANNEL_1);
			ccrl_val1b=HAL_TIM_ReadCapturedValue(htim,TIM_CHANNEL_2);
		__HAL_TIM_SetCounter(htim,0);
		frq1=(80000000/80)/ccrl_val1a;
		duty1=(ccrl_val1b/ccrl_val1a)*100;
		HAL_TIM_IC_Start(htim,TIM_CHANNEL_1);
		HAL_TIM_IC_Start(htim,TIM_CHANNEL_2);
		}
		
	}
	if(htim->Instance==TIM3)
	{
		if (htim->Channel==HAL_TIM_ACTIVE_CHANNEL_1)
		{
			ccrl_val2a=HAL_TIM_ReadCapturedValue(htim,TIM_CHANNEL_1);
			ccrl_val2b=HAL_TIM_ReadCapturedValue(htim,TIM_CHANNEL_2);
		__HAL_TIM_SetCounter(htim,0);
		frq2=(80000000/80)/ccrl_val2a;
		duty2=(ccrl_val2b/ccrl_val2a)*100;
		HAL_TIM_IC_Start(htim,TIM_CHANNEL_1);
		HAL_TIM_IC_Start(htim,TIM_CHANNEL_2);
		
		}
	}
}

char rxdata[30];
uint8_t rxdat;
uchar rx_pointer;
void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart)
{
	rxdata[rx_pointer++]=rxdat;
	HAL_UART_Receive_IT(&huart1,&rxdat,1);
	
	
}
