#pragma once

using namespace System;

typedef struct SpeexPreprocessState_ SpeexPreprocessState;

namespace SpeexEchoReducer
{
	public ref class EchoReducer : IDisposable
	{
	public:
		EchoReducer(int frameSize, int samplingRate);
		~EchoReducer();
		void EchoPlayback(array<short>^ echo_frame);
		void EchoCapture(array<short>^ input_frame, array<short>^ output_frame);
		void EchoPlayback(array<unsigned char>^ echo_frame);
		void EchoCapture(array<unsigned char>^ input_frame, array<unsigned char>^ output_frame);
		void EchoCapture(array<short>^ input_frame, array<unsigned char>^ output_frame);
		void EchoCancellation(array<unsigned char>^ input_frame, array<unsigned char>^ echo_frame, array<unsigned char>^ output_frame);
		void EchoCancellation(array<short>^ input_frame, array<unsigned char>^ echo_frame, array<unsigned char>^ output_frame);
	private:
		SpeexEchoState* _state;
	};
}
