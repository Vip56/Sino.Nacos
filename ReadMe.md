# Sino.Nacos
����Ŀ���ο�Nacos��[java�ͻ���](https://github.com/alibaba/nacos/tree/develop/client)���и�д������ֱ��ʹ�øü���ʵ�ַ����ע���뷢�֡�

## ����˵��

### �ֱ�֧��
���ǵ������Nacos����Ĺ��ϣ������ַ�����ֳ�ʱ���޷�ͨ�ŵ�������ڲ�ͨ����ʱ��ȡ�������µ����ݲ�������`cacheDir`·���µ�`failover`Ŀ¼�£�
�����������Ҫ���������л���ǰ��Ҫ����������ͨ��д���ֱ�Ŀ¼�µ�`00-00---000-VIPSRV_FAILOVER_SWITCH-000---00-00`�ļ����п��ƣ�������Ҫ
������SDKǰ��ָ��Ŀ¼�´������ļ���д���Ӧֵ��ͨ��"0"��"1"���ƣ�����"1"��������һ���������л�ȡ����ͨ���ֱ���������ݶ������������˳���    

### ��������
���ǵ�ʵ��ʹ�ø�SDK�ĳ����п��ܻ������ͬ����ͬʱע�᲻ͬ�ķ������ƻ�ע�᲻ͬ���������ҳ���IP�Ͷ˿ڲ�һ������������Ը�SDKʵ���˻���
��������+IP+Port����ķ��������Ա�֤Nacosע�����Ĳ��Ὣ����ע��ķ�����Ϊ������״̬��������Ҳ��ζ��ע����ཫ�ᵼ��һ����CPU�����ġ�    

### ʵ�����Ի�
��ʹ�ø�SDK���з���ע����������˻����Ĳ����⣬ͨ���趨ʵ���ľ���������Զ������һЩ�������ԣ�ֻ��ͨ��`Instance.MetaData`�ſ��ԣ���Ҫ
֧�ֵ��������£�
```
/// <summary>
/// �ṩʵ����������
/// </summary>
public class PreservedMetadataKeys
{
    /// <summary>
    /// ע����Դ
    /// </summary>
    public const string REGISTER_SOURCE = "preserved.register.source";

    /// <summary>
    /// ������ʱʱ��
    /// </summary>
    public const string HEART_BEAT_TIMEOUT = "preserved.heart.beat.timeout";

    /// <summary>
    /// ʵ���Ƴ���ʱʱ��
    /// </summary>
    public const string IP_DELETE_TIMEOUT = "preserved.ip.delete.timeout";

    /// <summary>
    /// �������ʱ��
    /// </summary>
    public const string HEART_BEAT_INTERVAL = "preserved.heart.beat.interval";
    }
```

## ����˵��

### �ݲ�֧�ֹ���
ͨ����д�Ĺ����п��ǵ���������ֹ��ܽ������ʵ���ɾ�����˸����Ϊ�˱�֤�͹ٷ�������SDK֮������ݱ���һ�£��ⲿ����ȫ�ο��ٷ�SDK������ƿ�����
������Ҫ�����¹��ܽ��е�ɾ����ƻ�δ��֧�֣�    
* ͨ��UDPЭ��ˢ�·�����Ϣ��
* HTTPʧ�����ԣ�
* ���÷���

### δ���ƻ�֧��
1. ֧��ʧ�����ԣ�
2. ֧�ֶ༶���棬���ǵ��������ļ���
3. ֧��ͨ����·��ʵ�������ֱ�֧�֣�

### �������
1. [mockhttp](https://github.com/richardszalay/mockhttp)��ʵ��HttpClientģ�⣻
2. [moq](https://github.com/moq/moq4)��ʵ�ֻ����ӿڷ���Mock��
3. [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)��Json���л�֧�֣�
4. [Nlog](https://github.com/NLog/NLog)���ṩ��־��¼��
5. [xunit](https://github.com/xunit/xunit)����Ԫ���Կ�ܣ�

### �����汾
ֻ֧��Asp.Net Core 2.1+��Ŀ

### �޶���¼
* 19.9.18 ���NamingProxy��EventDispatcher��Ԫ����;
