import { exactKeys, record } from './apiError';
import { validId } from '../features/administration/types';
export function object(value: unknown, keys: readonly string[]): Record<string, unknown> { if (!record(value) || !exactKeys(value, [...keys]))
    throw new Error('Invalid academic projection'); return value; }
export const text = (v: unknown): v is string => typeof v === 'string' && v.length > 0;
export const version = (v: unknown): v is string => typeof v === 'string' && /^[A-Za-z0-9+/]{10}[AEIMQUYcgkosw048]=$/.test(v);
export const instant = (v: unknown): v is string => typeof v === 'string' && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/.test(v) && Number.isFinite(Date.parse(v)) && new Date(v).toISOString() === v;
export const decimal = (v: unknown): v is string => typeof v === 'string' && /^\d+\.\d{2}$/.test(v);
export const strings = (v: unknown): v is string[] => Array.isArray(v) && v.every(text);
export function requireWire(valid: boolean): asserts valid { if (!valid)
    throw new Error('Invalid academic projection'); }
export function references(value: unknown) { requireWire(Array.isArray(value)); const ids = new Set<number>(); for (const row of value) {
    const r = object(row, ['id', 'version']);
    requireWire(validId(r.id) && version(r.version) && !ids.has(r.id));
    ids.add(r.id);
} }
export function capabilities(value: Record<string, unknown>, actions: readonly string[]) { requireWire(strings(value.allowedActions) && new Set(value.allowedActions).size === value.allowedActions.length && value.allowedActions.every(x => actions.includes(x)) && record(value.disabledReasons)); requireWire(Object.entries(value.disabledReasons).every(([key, reason]) => actions.includes(key) && !(value.allowedActions as string[]).includes(key) && text(reason)) && actions.every(key => (value.allowedActions as string[]).includes(key) || Object.hasOwn(value.disabledReasons as object, key))); }
export function course(value: unknown) { const r = object(value, ['id', 'code', 'name', 'status']); requireWire(validId(r.id) && text(r.code) && text(r.name) && ['ACTIVE', 'INACTIVE'].includes(r.status as string)); }
export function band(value: unknown) { const r = object(value, ['id', 'code', 'minScore']); requireWire(validId(r.id) && text(r.code) && decimal(r.minScore)); }
