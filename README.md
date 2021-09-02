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
```
