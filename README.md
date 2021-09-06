# Micro-Service-Demo
```
kubectl version
Client Version: version.Info{Major:"1", Minor:"21", GitVersion:"v1.21.2", GitCommit:"092fbfbf53427de67cac1e9fa54aaa09a28371d7", GitTreeState:"clean", BuildDate:"2021-06-16T12:59:11Z", GoVersion:"go1.16.5", Compiler:"gc", Platform:"windows/amd64"}

kubectl apply -f platforms-depl.yaml
deployment.apps/platforms-depl created

kubectl get deployments
NAME             READY   UP-TO-DATE   AVAILABLE   AGE
platforms-depl   1/1     1            1           41s

kubectl get pods
NAME                              READY   STATUS    RESTARTS   AGE
platforms-depl-687d4d5548-2prhk   1/1     Running   0          4m48s

kubectl delete deployment platforms-depl
deployment.apps "platforms-depl" deleted

kubectl apply -f platforms-np-srv.yaml
service/platformservice-srv created

kubectl get services
NAME                  TYPE        CLUSTER-IP       EXTERNAL-IP   PORT(S)        AGE
kubernetes            ClusterIP   10.96.0.1        <none>        443/TCP        23h
platformservice-srv   NodePort    10.111.229.151   <none>        80:32517/TCP   85s  //32571 is port for k8s

kubectl rollout restart deployment platforms-depl
deployment.apps/platforms-depl restarted

kubectl apply -f commands-depl.yaml
deployment.apps/commands-depl created
service/commands-clusterip-srv created

kubectl get services
NAME                      TYPE        CLUSTER-IP       EXTERNAL-IP   PORT(S)        AGE
commands-clusterip-srv    ClusterIP   10.108.210.199   <none>        80/TCP         76s
kubernetes                ClusterIP   10.96.0.1        <none>        443/TCP        47h
platforms-clusterip-srv   ClusterIP   10.104.55.64     <none>        80/TCP         17m
platformservice-srv       NodePort    10.111.229.151   <none>        80:32517/TCP   24h

kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.0.0/deploy/static/provider/cloud/deploy.yaml

kubectl get namespace
NAME              STATUS   AGE
default           Active   2d
ingress-nginx     Active   2m9s
kube-node-lease   Active   2d
kube-public       Active   2d
kube-system       Active   2d

kubectl get pods --namespace=ingress-nginx
NAME                                       READY   STATUS      RESTARTS   AGE
ingress-nginx-admission-create-nft2f       0/1     Completed   0          4m20s
ingress-nginx-admission-patch-6t6l4        0/1     Completed   1          4m20s
ingress-nginx-controller-fd7bb8d66-jxfrc   1/1     Running     0          4m22s

kubectl apply -f ingress-srv.yaml
ingress.networking.k8s.io/ingress-srv created

kubectl get storageclass
NAME                 PROVISIONER          RECLAIMPOLICY   VOLUMEBINDINGMODE   ALLOWVOLUMEEXPANSION   AGE
hostpath (default)   docker.io/hostpath   Delete          Immediate           false                  2d22h

kubectl apply -f local-pvc.yaml
persistentvolumeclaim/mssql-claim created

kubectl get pvc
NAME          STATUS   VOLUME                                     CAPACITY   ACCESS MODES   STORAGECLASS   AGE
mssql-claim   Bound    pvc-9eee3ac9-b657-4068-9606-bcc33a7eee4e   200Mi      RWX            hostpath       74s

kubectl get storageclass
NAME                 PROVISIONER          RECLAIMPOLICY   VOLUMEBINDINGMODE   ALLOWVOLUMEEXPANSION   AGE
hostpath (default)   docker.io/hostpath   Delete          Immediate           false                  2d22h

kubectl create secret generic mssql --from-literal=SA_PASSWORD="xxxxx"
secret/mssql created

kubectl apply -f mssql-plat-depl.yaml
deployment.apps/mssql-depl created
service/mssql-clusterip-srv created
service/mssql-loadbalance created

kubectl get services
 NAME                      TYPE           CLUSTER-IP       EXTERNAL-IP   PORT(S)          AGE
commands-clusterip-srv    ClusterIP      10.108.210.199   <none>        80/TCP           23h
kubernetes                ClusterIP      10.96.0.1        <none>        443/TCP          2d23h
mssql-clusterip-srv       ClusterIP      10.111.155.104   <none>        1433/TCP         112s
mssql-loadbalance         LoadBalancer   10.96.227.145    localhost     1433:32537/TCP   111s
platforms-clusterip-srv   ClusterIP      10.104.55.64     <none>        80/TCP           23h
platformservice-srv       NodePort       10.111.229.151   <none>        80:32517/TCP     47h

kubectl get pods
NAME                             READY   STATUS    RESTARTS   AGE
commands-depl-c4fcc556b-htfvc    1/1     Running   2          23h
mssql-depl-856b8c48fd-qg4w6      1/1     Running   2          12m
platforms-depl-7d9588c8f-lgftv   1/1     Running   2          24h

kubectl apply -f .\rabbitmq-depl.yaml
deployment.apps/rabbitmq-depl created
service/rabbitmq-clusterip-srv created
service/rabbitmq-loadbalance created

kubectl get services
NAME                      TYPE           CLUSTER-IP       EXTERNAL-IP   PORT(S)                          AGE
commands-clusterip-srv    ClusterIP      10.108.210.199   <none>        80/TCP                           4d8h
kubernetes                ClusterIP      10.96.0.1        <none>        443/TCP                          6d7h
mssql-clusterip-srv       ClusterIP      10.104.15.29     <none>        1433/TCP                         2d6h
mssql-loadbalance         LoadBalancer   10.111.0.230     localhost     1433:30153/TCP                   2d6h
platforms-clusterip-srv   ClusterIP      10.104.55.64     <none>        80/TCP                           4d8h
platformservice-srv       NodePort       10.111.229.151   <none>        80:32517/TCP                     5d8h
rabbitmq-clusterip-srv    ClusterIP      10.108.157.223   <none>        15672/TCP,5672/TCP               37s
rabbitmq-loadbalance      LoadBalancer   10.103.7.24      localhost     15672:32267/TCP,5672:31001/TCP   37s

kubectl get deployments
NAME             READY   UP-TO-DATE   AVAILABLE   AGE
commands-depl    1/1     1            1           4d8h
mssql-depl       1/1     1            1           2d6h
platforms-depl   1/1     1            1           2d7h
rabbitmq-depl    1/1     1            1           98s

kubectl get pods
NAME                              READY   STATUS    RESTARTS   AGE
commands-depl-c4fcc556b-htfvc     1/1     Running   8          4d8h
mssql-depl-856b8c48fd-4bf84       1/1     Running   4          2d6h
platforms-depl-687d4d5548-gzf52   1/1     Running   8          2d7h
rabbitmq-depl-76f9ff665c-5v5l8    1/1     Running   0          106s
```
