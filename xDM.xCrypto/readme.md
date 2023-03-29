# xDM.xCrypto

## 加解密的C#实现，支持对称(RSA)/非对称(SM4 DES 3DES AES RC2 DSA)加解密，摘要算法（MD5 SHA1 SHA256 SHA384 SHA512 SM3）

### 主要功能由静态类 XCryptor提供

#### 对称加密，目前支持 国密算法：SM4     通用算法：DES 3DES AES RC2 DSA

##### 使用方法：

    var key = "123";
    var keyBytes = key.ToBytes();
    var iv = "654987";
    var ivBytes = iv.ToBytes();
    var file = "file.txt"; //要加密的文件
    var secretFile = "secret.txt";//加密后的文件
    var noSecretFile = "no_secret.txt";//解密后的文件
    var text = "测试加密";
    var data = new byte[] { 0x1, 0x2, 0xff, 0x9 };

    /*简单使用*/

    /*----核心方法----*/
    /*
        * 核心方法只有4个：加/解密二进制数据和加/解密文件，
        * 其他的都是从这4个方法扩展而来
    */
    //加密二进制数据
    var secretData = XCryptor.Aes.EncryptData(data, keyBytes);
    //解密二进制数据
    var sData = XCryptor.Aes.DecryptData(secretData, keyBytes);

    //加密文件
    XCryptor.Aes.EncryptFile(file, secretFile, keyBytes);
    //解密文件
    XCryptor.Aes.DecryptFile(secretFile, noSecretFile, keyBytes);
    /*----核心方法----*/

    /*----扩展方法----/
        * 扩展方法由核心方法通过 xExtensions 的扩展方法实现
        * 如果不清楚 xExtemsions 扩展方法是什么，请无视
    */
    //加密字符串
    var base64String = XCryptor.Aes.EncryptToBase64(text, key);
    var hexString = XCryptor.Aes.EncryptToHexString(text, key);

    //解密字符串
    text = XCryptor.Aes.DecryptFromBase64(base64String, key);
    text = XCryptor.Aes.DecryptFromHexString(hexString, key);
    /*----扩展方法----*/


    /*高级用法*/
    /*
        * 在高级用法里，你可以指定填充方式paddingMode，初始向量iv
        * 如果不清楚这两个参数意义，请查阅对称加密相关文档
        * 初始向量iv: 一般CBC模式需要，ECB模式请设为null
        * 填充类型 paddingMode : 默认为 ISO10126，对于各个类型的说明，请参阅相关文档
    */
    /*----核心方法----*/
    //加密二进制数据

    //public byte[] EncryptData(byte[] data, byte[] key, byte[] iv = null, PaddingMode paddingMode = PaddingMode.ISO10126)
    secretData = XCryptor.Aes.EncryptData(data, keyBytes, ivBytes, PaddingMode.ISO10126);
    //解密二进制数据
    sData = XCryptor.Aes.DecryptData(secretData, keyBytes, ivBytes, PaddingMode.ISO10126);

    //加密文件
    XCryptor.Aes.EncryptFile(file, secretFile, keyBytes, ivBytes, PaddingMode.ISO10126);
    //解密文件
    XCryptor.Aes.DecryptFile(secretFile, noSecretFile, keyBytes, ivBytes, PaddingMode.ISO10126);
    /*----核心方法----*/

#### 非对称加密 目前支持 通用算法：RSA
RSA密钥支持巨硬的xml格式字符串及pem格式字符串
支持使用私钥加密公钥解密（什么？你非要这么反人类？），RSA的效率感人（因为是自己实现的算法，在 BigInteger.ModPow(biData, d_e, modulus)
里效率简直。。。）如果你一定要这样做，请把加密时指定 usePrivate = true, 解密时指定 usePublic = true

##### 使用方法：
    var key = "123";
    var keyBytes = key.ToBytes();
    var iv = "654987";
    var ivBytes = iv.ToBytes();
    var file = "file.txt"; //要加密的文件
    var secretFile = "secret.txt";//加密后的文件
    var noSecretFile = "no_secret.txt";//解密后的文件
    var text = "测试加密";
    var data = new byte[] { 0x1, 0x2, 0xff, 0x9 };

    /*简单使用*/

    /*----核心方法----*/
    /*
    * 核心方法只有4个：加/解密二进制数据和加/解密文件，
    * 其他的都是从这4个方法扩展而来
    * 
    * 采用正常的公钥加密，私钥解密模式
    */
    //加密二进制数据
    var keyPart = XCryptor.Rsa.GenerateSecretKey(512, RsaKeyType.xml);
    var secretData = XCryptor.Rsa.EncryptData(data, keyPart.PublicKey);
    //解密二进制数据
    var sData = XCryptor.Rsa.DecryptData(secretData, keyPart.PrivateKey);

    //加密文件
    XCryptor.Rsa.EncryptFile(file, secretFile, keyPart.PublicKey);
    //解密文件
    XCryptor.Rsa.DecryptFile(secretFile, noSecretFile, keyPart.PrivateKey);
    /*----核心方法----*/

#### 摘要算法，目前支持 国密算法：SM3    通用算法：MD5 SHA1 SHA256 SHA384 SHA512

##### 使用方法：

    var text = "测试哈希";
    var file = "file.txt"; 

    /*----核心方法----*/
    //文件哈希
    var md5 = XCryptor.GetFileHash(file, HashType.MD5);
    var md5_string = XCryptor.GetFileHashString(file, HashType.MD5)

    //字符串哈希
    md5 = XCryptor.GetHash(text, HashType.MD5);
    var md5_string = XCryptor.GetHashString(file, HashType.MD5)

    /*----扩展方法----*/
    //在扩展的加持下，你可以这样子用：
    md5 = text.GetHash(HashType.MD5);
    var sha1_string = text.GetHashString(HashType.SHA1);