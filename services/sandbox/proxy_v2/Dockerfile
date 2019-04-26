FROM python:2.7-slim

RUN pip install tornado bottle

ADD . /root/proxy
WORKDIR /root/proxy

CMD ["python", "proxy.py"]