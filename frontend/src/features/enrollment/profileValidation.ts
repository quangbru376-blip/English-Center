import type { ProfileFields } from './components/StudentProfileForm';
// Hints only. The server repeats normalization, uniqueness and Vietnam date guards.
export function validProfileDraft(draft: ProfileFields, now = new Date()) {
    const name = draft.fullName.normalize('NFC').trim(), address = draft.address.normalize('NFC').trim();
    const phone = draft.phone.trim().replace(/[\p{Zs}-]/gu, '').replace(/^\+84/, '0');
    const parts = new Intl.DateTimeFormat('en-CA', { timeZone: 'Asia/Ho_Chi_Minh', year: 'numeric', month: '2-digit', day: '2-digit' }).formatToParts(now);
    const today = ['year', 'month', 'day'].map(key => parts.find(p => p.type === key)!.value).join('-');
    return !!name && Array.from(name).length <= 100 && Array.from(address).length <= 255 && /^0[35789]\d{8}$/.test(phone) &&
        /^(?!0000)\d{4}-\d{2}-\d{2}$/.test(draft.dateOfBirth) && Number.isFinite(Date.parse(draft.dateOfBirth + 'T00:00:00Z')) &&
        new Date(draft.dateOfBirth + 'T00:00:00Z').toISOString().slice(0, 10) === draft.dateOfBirth && draft.dateOfBirth <= today;
}
