.text
main:
	move $fp, $sp
	sub $sp, $sp, 52
L24:
	sw $ra, -28($fp)
	sw $s7, -24($fp)
	li $t0, 8
	sw $t0, -4($fp)
	add $t0, $fp, -8
	sw $t0, -36($fp)
	lw $a0, -4($fp)
	move $a1, $zero
	jal _array
	add $fp, $sp, 52
	lw $t0, -36($fp)
	sw $v0, 0($t0)
	add $t0, $fp, -12
	sw $t0, -40($fp)
	lw $a0, -4($fp)
	move $a1, $zero
	jal _array
	add $fp, $sp, 52
	lw $t0, -40($fp)
	sw $v0, 0($t0)
	add $t0, $fp, -16
	sw $t0, -44($fp)
	lw $t0, -4($fp)
	lw $t1, -4($fp)
	add $t0, $t0, $t1
	sub $a0, $t0, 1
	move $a1, $zero
	jal _array
	add $fp, $sp, 52
	lw $t0, -44($fp)
	sw $v0, 0($t0)
	add $t0, $fp, -20
	sw $t0, -48($fp)
	lw $t1, -4($fp)
	lw $t0, -4($fp)
	add $t0, $t1, $t0
	sub $a0, $t0, 1
	move $a1, $zero
	jal _array
	add $fp, $sp, 52
	lw $t0, -48($fp)
	sw $v0, 0($t0)
	sw $fp, 0($sp)
	move $a0, $zero
	jal try_2
	add $fp, $sp, 52
	lw $s7, -24($fp)
	lw $ra, -28($fp)
	j L23
L23:
	move $sp, $fp
	j _exit
.text
try_2:
	move $fp, $sp
	sub $sp, $sp, 36
L26:
	sw $a0, -16($fp)
	sw $ra, -12($fp)
	sw $s7, -8($fp)
	sw $s6, -4($fp)
	lw $t0, 0($fp)
	lw $t0, -4($t0)
	lw $t1, -16($fp)
	beq $t1, $t0, L13
L14:
	li $s7, 0
	lw $t0, 0($fp)
	lw $t0, -4($t0)
	sub $t0, $t0, 1
	move $s6, $t0
	ble $s7, $s6, L15
L11:
L12:
	lw $s6, -4($fp)
	lw $s7, -8($fp)
	lw $ra, -12($fp)
	j L25
L13:
	lw $t0, 0($fp)
	sw $t0, 0($sp)
	jal printboard_1
	add $fp, $sp, 36
	j L12
L15:
	lw $t0, 0($fp)
	lw $t0, -8($t0)
	sll $t1, $s7, 2
	add $t0, $t0, $t1
	lw $t0, 0($t0)
	beq $t0, 0, L21
L22:
L20:
L16:
	add $t0, $s7, 1
	move $s7, $t0
	ble $s7, $s6, L15
L27:
	j L11
L21:
	lw $t0, 0($fp)
	lw $t1, -16($t0)
	lw $t0, -16($fp)
	add $t0, $s7, $t0
	sll $t0, $t0, 2
	add $t0, $t1, $t0
	lw $t0, 0($t0)
	bne $t0, 0, L20
L19:
	lw $t0, 0($fp)
	lw $t2, -20($t0)
	add $t1, $s7, 7
	lw $t0, -16($fp)
	sub $t0, $t1, $t0
	sll $t0, $t0, 2
	add $t0, $t2, $t0
	lw $t0, 0($t0)
	bne $t0, 0, L16
L17:
	lw $t0, 0($fp)
	lw $t1, -8($t0)
	sll $t0, $s7, 2
	add $t1, $t1, $t0
	li $t0, 1
	sw $t0, 0($t1)
	lw $t0, 0($fp)
	lw $t1, -16($t0)
	lw $t0, -16($fp)
	add $t0, $s7, $t0
	sll $t0, $t0, 2
	add $t1, $t1, $t0
	li $t0, 1
	sw $t0, 0($t1)
	lw $t0, 0($fp)
	lw $t2, -20($t0)
	add $t1, $s7, 7
	lw $t0, -16($fp)
	sub $t0, $t1, $t0
	sll $t0, $t0, 2
	add $t1, $t2, $t0
	li $t0, 1
	sw $t0, 0($t1)
	lw $t0, 0($fp)
	lw $t1, -12($t0)
	lw $t0, -16($fp)
	sll $t0, $t0, 2
	add $t0, $t1, $t0
	sw $s7, 0($t0)
	lw $t1, 0($fp)
	lw $t0, -16($fp)
	add $a0, $t0, 1
	sw $t1, 0($sp)
	jal try_2
	add $fp, $sp, 36
	lw $t0, 0($fp)
	lw $t1, -8($t0)
	sll $t0, $s7, 2
	add $t0, $t1, $t0
	sw $zero, 0($t0)
	lw $t0, 0($fp)
	lw $t1, -16($t0)
	lw $t0, -16($fp)
	add $t0, $s7, $t0
	sll $t0, $t0, 2
	add $t0, $t1, $t0
	sw $zero, 0($t0)
	lw $t0, 0($fp)
	lw $t2, -20($t0)
	add $t1, $s7, 7
	lw $t0, -16($fp)
	sub $t0, $t1, $t0
	sll $t0, $t0, 2
	add $t0, $t2, $t0
	sw $zero, 0($t0)
	j L16
L25:
	move $sp, $fp
	jr $ra
.text
printboard_1:
	move $fp, $sp
	sub $sp, $sp, 44
L29:
	sw $ra, -20($fp)
	sw $s7, -16($fp)
	sw $s6, -12($fp)
	sw $s5, -8($fp)
	sw $s4, -4($fp)
	li $s6, 0
	lw $t0, 0($fp)
	lw $t0, -4($t0)
	sub $t0, $t0, 1
	move $s7, $t0
	ble $s6, $s7, L10
L0:
	la $a0, L9
	jal _print
	add $fp, $sp, 44
	lw $s4, -4($fp)
	lw $s5, -8($fp)
	lw $s6, -12($fp)
	lw $s7, -16($fp)
	lw $ra, -20($fp)
	j L28
L10:
	li $s4, 0
	lw $t0, 0($fp)
	lw $t0, -4($t0)
	sub $t0, $t0, 1
	move $s5, $t0
	ble $s4, $s5, L8
L1:
	la $a0, L7
	jal _print
	add $fp, $sp, 44
	add $t0, $s6, 1
	move $s6, $t0
	ble $s6, $s7, L10
L30:
	j L0
L8:
	lw $t0, 0($fp)
	lw $t1, -12($t0)
	sll $t0, $s6, 2
	add $t0, $t1, $t0
	lw $t0, 0($t0)
	beq $t0, $s4, L5
L6:
	la $a0, L3
L4:
	jal _print
	add $fp, $sp, 44
	add $t0, $s4, 1
	move $s4, $t0
	ble $s4, $s5, L8
L31:
	j L1
L5:
	la $a0, L2
	j L4
L28:
	move $sp, $fp
	jr $ra
.data
L9:
	.asciiz "\n"

.data
L7:
	.asciiz "\n"

.data
L3:
	.asciiz " ."

.data
L2:
	.asciiz " O"

# runtime #
.text
_print:
	li $v0, 4
	syscall
	jr $ra

_printi:
	li $v0, 1
	syscall
	jr $ra

_flush:
	jr $ra

_getchar:
	li $v0, 9 
	li $a0, 2
	syscall
	move $a0, $v0
	li $a1, 2
	li $v0, 8
	syscall
	move $v0, $a0
	jr $ra

_ord:
	lb $a1, ($a0)
	li $v0, -1
	beqz $a1, _ord_1
	lb $v0, ($a0)
_ord_1:
	jr $ra

_chr:
	move $a1, $a0
	li $v0, 9
	li $a0, 2
	syscall
	sb $a1, ($v0)
	sb $zero, 1($v0)
	jr $ra

_size:
	move $v0, $zero
_size_1:
	lb $a1, ($a0)
	beq $a1, $zero, _size_2
	add $v0, $v0, 1
	add $a0, $a0, 1
	j _size_1
_size_2:
	jr $ra

_substring:
	add $a1, $a0, $a1
	move $a3, $a1
	li $v0, 9
	add $a2, $a2, 1
	move $a0, $a2
	add $a0, $a0, 1 
	syscall
	add $a2, $a2, $a3
	add $a2, $a2, -1
	move $a0, $v0
_substring_1:
	beq $a1, $a2, _substring_2
	lb $a3, ($a1)
	sb $a3, ($a0)
	add $a1, $a1, 1
	add $a0, $a0, 1 
	j _substring_1
_substring_2:
	sb $zero, ($a0)
	jr $ra

_concat:
	sw $a0, -4($sp)
	sw $a1, -8($sp)
	sw $ra, -12($sp)
	jal _size
	li $a3, 1
	add $a3, $a3, $v0
	lw $a0, -8($sp)
	jal _size
	add $a3, $a3, $v0
	move $a0, $a3
	li $v0, 9
	syscall 
	move $a3, $v0
	move $a0, $v0
	lw $a1, -4($sp)
	jal _copy
	move $a0, $v0
	lw $a1, -8($sp)
	jal _copy
	move $v0, $a3
	lw $ra, -12($sp)
	jr $ra

_not:
	seq $v0, $a0, 0
	jr $ra

_exit:
	li $v0, 10
	syscall

_strcmp:
_strcmp_1:
	lb $a2, ($a0)
	lb $a3, ($a1)
	beq $a2, $zero, _strcmp_4
	beq $a3, $zero, _strcmp_4
	bgt $a2, $a3, _strcmp_2
	blt $a2, $a3, _strcmp_3
	add $a0, $a0, 1
	add $a1, $a1, 1
	j _strcmp_1
_strcmp_2:
	li $v0, 1
	jr $ra
_strcmp_3:
	li $v0, -1
	jr $ra
_strcmp_4:
	bne $a2, $zero, _strcmp_2
	bne $a3, $zero, _strcmp_3
	li $v0, 0
	jr $ra

_record:
	li $v0, 9
	syscall
	move $v1, $v0
	add $a0, $a0, $v0
_record_1:
	sw $zero, ($v1)
	add $v1, $v1, 4
	bne $v1, $a0, _record_1
	jr $ra

_array:
	sll $a0, $a0, 2
	li $v0, 9
	syscall
	move $v1, $v0
	add $a0, $a0, $v0
_array_1:
	sw $a1, ($v1)
	add $v1, $v1, 4
	bne $v1, $a0, _array_1
	jr $ra

_malloc:
	li $v0,9
	syscall
	jr $ra

_copy:
_copy_1:
	lb $a2, ($a1)
	beq $zero, $a2 _copy_2
	sb $a2, ($a0)   
	add $a0, $a0, 1
	add $a1, $a1, 1
	j _copy_1
_copy_2:
	sb $zero, ($a0)
	move $v0, $a0
	jr $ra
