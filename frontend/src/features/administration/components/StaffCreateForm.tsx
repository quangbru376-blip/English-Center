import { useState } from 'react';
import { Button } from '../../../components/ui/Button';
import { TextField } from '../../../components/ui/TextField';
import { SelectField } from '../../../components/ui/SelectField';
import type { StaffFields } from '../staffApi';
export function StaffCreateForm({pending,onSubmit,onCancel}:{pending:boolean;onSubmit:(value:StaffFields)=>void;onCancel:()=>void}){
  const [name,setName]=useState(''),[email,setEmail]=useState(''),[phone,setPhone]=useState(''),[role,setRole]=useState('Teacher');
  return <form className="space-y-3" onSubmit={event=>{event.preventDefault();if(!pending)onSubmit({fullName:name,email,role,...phone.trim()?{phone}: {}});}}>
    <TextField label="Họ và tên" value={name} onChange={e=>setName(e.currentTarget.value)} required disabled={pending}/>
    <TextField label="Email đăng nhập" type="email" value={email} onChange={e=>setEmail(e.currentTarget.value)} required disabled={pending}/>
    <TextField label="Số điện thoại (không bắt buộc)" value={phone} onChange={e=>setPhone(e.currentTarget.value)} disabled={pending}/>
    <SelectField label="Vai trò" value={role} onChange={e=>setRole(e.currentTarget.value)} disabled={pending}>
      {Object.entries({Admissions:'Tuyển sinh',AcademicAffairs:'Giáo vụ',Teacher:'Giáo viên',Accountant:'Kế toán',Director:'Giám đốc',Admin:'Quản trị viên'}).map(([code,label])=><option key={code} value={code}>{label}</option>)}
    </SelectField>
    <Button type="submit" pending={pending}>Cấp tài khoản</Button> <Button variant="secondary" disabled={pending} onClick={onCancel}>Hủy</Button>
  </form>;
}
