//#define TAB_LEN 256
//#define ALPHA 0x09
//int table_gen8(unsigned char *buf) {
//	unsigned int alpha = ALPHA;  //x^7+x^3+1
//	int i, j;
//	unsigned char tmp;
//	for (i = 0; i < TAB_LEN; i++) {
//		tmp = i;
//		for (j = 0; j < 8; j++) {
//			if (tmp & 0x80)
//				tmp ^= alpha;
//			tmp <<= 1;
//		}
//		buf[i] = tmp >> 1;    /*余数为7bit，计算中用了8bit，结尾多一位0要去掉*/
//		// 取前面的几位【如果选取的余数的位数是8，那么取前面7位】
//	}
//	return 0;
//}
//
//unsigned char get_crc7(unsigned char start, const unsigned char *buff, int len, unsigned char *table) {
//	unsigned char accu = start;
//	unsigned int i = 0;
//	for (i = 0; i < len; i++) {
//		accu = table[(accu << 1) ^ buff[i]];
//	}
//	return accu;
//}
//
//using namespace std;
//
//int main()
//{
//	unsigned char data[TAB_LEN] = { 0 };
//	int i, j;
//	printf("CRC7 table:\n");
//	table_gen8(data);
//	for (i = 0; i < TAB_LEN / 8; i++) {
//		for (j = 0; j < 8; j++)
//			printf("0x%02x   ", data[i * 8 + j]);
//		printf("\n");
//	}
//	printf("\n");
//	/*Test*/
//	const unsigned char testdat[] = "0123456789";
//	unsigned char result;
//	result = get_crc7(0, testdat, (sizeof(testdat) / sizeof(char) - 1), data);
//	printf("get_crc7:0x%02x\n", result);
//
//	system("pause");
//	return 0;
//}