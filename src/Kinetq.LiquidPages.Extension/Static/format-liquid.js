import prettier from 'prettier';
import liquidPlugin from '@shopify/prettier-plugin-liquid';

let inputData = '';

process.stdin.setEncoding('utf8');
process.stdin.on('data', chunk => (inputData += chunk));
process.stdin.on('end', async () => {
    try {
        const formatted = await prettier.format(inputData, {
            parser: 'liquid-html',
            plugins: [liquidPlugin],
        });
        process.stdout.write(formatted);
    } catch (e) {
        console.error(e.message);
        process.exit(1);
    }
});
