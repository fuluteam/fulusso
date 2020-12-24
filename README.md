## 背景

18年公司准备在技术上进行转型，而公司技术团队是互相独立的，新技术的推动阻力很大。我们需要找到一个切入点。公司的项目很多，而各个系统之间又不互通，导致每套系统都有一套登录体系，给员工和客户都带来极大的不便。那么从登录切入进去无疑最合适，对于各个团队的技术改造成本也不大。所以我们团队第一个项目就是搭建一套统一登录认证授权系统，那么葫芦藤项目应运而生。

## 技术方案

后端框架：.NET Core3.1（后期会推出 .NET 5版本）

前端框架：React 

数据库：mysql（可根据实际情况，自由切换）

中间件：redis

## 详细功能




####  认证授权服务

基于IdentityServer4实现的协议，支持网站、本地应用、移动端、web服务等应用的认证授权逻辑。

####  单点登录登出

支持各种类型应用上的单点登录登出。开箱即用的基础用户管理模块，包括：注册、登录、手机验证码、忘记密码等。为了安全考虑，集成了腾讯图形验证码。

####  第三方登录（微信、钉钉）

完善的第三方登录支持。支持首次登录时绑定已存在用户或注册新用户后，自动绑定。




## 如何快速使用

#### 1.下载代码

clone代码到本地。根目录结构如下：

![20201103153907](https://fulu-item11-zjk.oss-cn-zhangjiakou.aliyuncs.com/images/20201103153907.png)

其中，backend存放的是后端代码，frontend存放的是前端代码。

进入backend目录，使用Visual Studio打开解决方案。目录结构如下：

![20201103154250](https://fulu-item11-zjk.oss-cn-zhangjiakou.aliyuncs.com/images/20201103154250.png)

#### 2.生成数据库

首先在Fulu.Passport.Web中找到appsettings.Development.json文件。编辑数据库连接字符串：

![20201103155350](https://fulu-item11-zjk.oss-cn-zhangjiakou.aliyuncs.com/images/20201103155350.png)


打开程序包管理器，切换默认项目为：Fulu.Passport.Web, 如下图所示：


![20201106111334](https://fulu-item11-zjk.oss-cn-zhangjiakou.aliyuncs.com/images/20201106111334.png)


然后在程序包管理器中执行如下命令：

```

Add-Migration Init
```
最后执行完成后，再执行如下命令：
```
update-database
```
执行完以上操作后，如没有报错，则会创建数据库，并会在Client表中创建一条测试数据，如下图所示：

![20201103160408](https://fulu-item11-zjk.oss-cn-zhangjiakou.aliyuncs.com/images/20201103160408.png)



#### 3.按F5启动后端服务

注：由于项目中依赖redis来处理缓存，所以正式启动之前，需要将appsettings.Development.json文件里的redis配置改为你自己的。

#### 4.启动前端

切换目录到frontend，在命令行中执行如下命令：

```
npm install
```
执行完毕后，执行如下命令：

```
npm run demo
```

执行结果如下图所示：

![20201103161300](https://fulu-item11-zjk.oss-cn-zhangjiakou.aliyuncs.com/images/20201103161300.png)

然后通过http://localhost:8080进行访问。界面如下所示：

![20201103174200](https://fulu-item11-zjk.oss-cn-zhangjiakou.aliyuncs.com/images/20201103174200.png)

至此，前后端服务已启动完毕，一个开箱即用的认证授权服务就完成了。

#### 5.新客户端如何快速接入认证服务？

认证授权服务存在的意义就是提供统一的认证授权入口，有了这个服务后，每个新的客户端应用无需单独开发认证授权模块。下面就来一起看下如何快速将新应用接入到认证授权服务。（*此处以 ASP.NET Core作为示例，其他语言大同小异*）。

示例代码在sample文件夹中，如下图所示：

![20201104165955](https://fulu-item11-zjk.oss-cn-zhangjiakou.aliyuncs.com/images/20201104165955.png)

在正式接入之前，必须先申请应用。（此版本未提供应用管理服务）通过在数据库中添加示例信息，如下图所示：

![20201104192124](https://fulu-item11-zjk.oss-cn-zhangjiakou.aliyuncs.com/images/20201104192124.png)

示例sql脚本：


```
INSERT INTO `fulusso`.`client`(`client_secret`, `full_name`, `host_url`, `redirect_uri`, `description`, `enabled`, `id`) VALUES ('14p9ao1gxu4q3sp8ogk8bq4gkct59t9w', '葫芦藤2', 'http://localhost:5003/', 'http://localhost:5003', NULL, 1, UUID());

```
其中，redirect_uri参数指的是从认证服务获取code之后，重定向的url。为了开发的方便，我们的认证服务中仅校验回调域名的域名，不会校验完整的地址。比如，你的redirect_uri为http://www.xxx.com/abc/aaa，则数据库中的redirect_uri字段填写http://www.xxx.com即可。

应用信息导入到数据库后，在Startup类的ConfigureServices方法中，添加如下代码：

```
services.AddServiceAuthorize(o =>
{
    o.AllowClientToken = true;
    o.AllowUserToken = true;
    o.OnClientValidate = false;
    o.Authority = "http://localhost:5000";
    o.ValidateAudience = false;
    o.ClientId = Configuration["AppSettings:ClientId"];
    o.ClientSecret = Configuration["AppSettings:ClientSecret"];
});
```
注：需添加Fulu.Service.Authorize项目引用，如下图所示：

![20201104170401](https://fulu-item11-zjk.oss-cn-zhangjiakou.aliyuncs.com/images/20201104170401.png)

然后在Configure方法中，添加如下代码：


```
 app.UseRouting();
 app.UseJwtAuthorize();
 app.UseAuthorization();
```

其中，UseJwtAuthorize是自定义的中间件，为了实现OAuth2.0的授权码的逻辑。
限于篇幅，具体代码不在此列出。可在代码仓库中查看。

到此为止，这个新应用就成功的接入到认证服务了。

当未登录的时候，访问此应用的页面会自动跳转到认证服务的login界面。登录之后，会重定向回登录之前的页面。如下图所示：

![aa](https://fulu-item11-zjk.oss-cn-zhangjiakou.aliyuncs.com/images/aa.gif)



## 下一版功能规划

1.更多的第三方平台的接入（QQ、微博等）

2.api授权服务

3.更安全的二次验证，集成google令牌

4.应用管理

等等~~~~，尽请期待。

## 体验


演示地址：https://account.suuyuu.cn/

代码仓库：https://github.com/fuluteam/fulusso

博客地址：https://www.cnblogs.com/fulu

---


#### 如果觉得项目对于有所帮助，欢迎star。您的支持是我们持续更新的动力。

