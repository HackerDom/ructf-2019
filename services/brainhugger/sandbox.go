package main

import (
	"fmt"
	"sync"
)

type Locker struct {
	locks map[int]bool
	mux  sync.Mutex
}

func spin(locker *Locker, id int) {
	locker.mux.Lock()
	defer locker.mux.Unlock()
	locker.data[id] = true
	// ....
	locker.data[id] = false
}

func main() {
	var a A
	a.data = make(map[int]int)
	for i := 0; i < 45; i++ {
		go f(&a)
	}
	fmt.Println(a.data)
}
