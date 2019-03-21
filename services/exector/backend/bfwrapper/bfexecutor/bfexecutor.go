package main

/*
#cgo CFLAGS: -O0 -fno-stack-protector
#cgo LDFLAGS: ${SRCDIR}/lib/bfexecutor.a -lm
#include <bfexecutor.h>
 */
import "C"
import (
	"io/ioutil"
	"os"
	"unsafe"
)

const MaxOutputLength = 1024

var returnCodeToError = map[int]string {
	1: "Output limit exceeded",
	2: "Input exceeded",
	3: "Incorrect brackets",
	4: "Incorrect brackets",
	5: "Operations limit exceeded",
	6: "Incorrect brackets",
}

func runBfCode(code string, input []byte, maxOperations uint) ([]byte, int) {
	if len(input) == 0 {
	    input = []byte{0}
	}
	var bcode = []byte(code)
	codePtr := (*C.char)(unsafe.Pointer(&bcode[0]))
	codeLen := C.int(len(bcode))

	var binput = input
	inputPtr := (*C.char)(unsafe.Pointer(&binput[0]))
	inputLen := C.int(len(binput))

	var boutput [MaxOutputLength]byte
	outputPtr := (*C.char)(unsafe.Pointer(&boutput[0]))

	maxOutLen := C.int(MaxOutputLength)

	cmaxOperations := C.uint(maxOperations)

	var writtenBytes uint
	writtenBytesPtr := (*C.uint)(unsafe.Pointer(&writtenBytes))

	retCode := C.run_bf_code(codePtr, codeLen, inputPtr, inputLen, outputPtr, maxOutLen, cmaxOperations, writtenBytesPtr)

	if retCode != 0 {
		return []byte{}, int(retCode)
	}
	return boutput[:writtenBytes], 0
}

func main() {
	data, err := ioutil.ReadAll(os.Stdin)
	if err != nil {
		os.Exit(101)
	}
	if len(os.Args) != 2 {
		os.Exit(102)
	}
	result, retCode := runBfCode(os.Args[1], data, 50000)
	if retCode != 0 {
		_, err = os.Stderr.Write([]byte(returnCodeToError[int(retCode)]))
		if err != nil {
			os.Exit(103)
		}
		os.Exit(retCode)
	}
	if _, err := os.Stdout.Write(result); err != nil {
		os.Exit(104)
	}
}
